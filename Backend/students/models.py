from django.db import models, transaction, IntegrityError
from django.contrib.auth.models import User
from classes.models import Class
from quizzes.models import Quiz
from django.core.exceptions import ValidationError
from quizzes.helpers import QuizGradingHelper


class Student(models.Model):
    """
    Represents a student who joins a class using a valid class code.
    """
    full_name = models.CharField(max_length=255)
    email = models.EmailField(blank=True, null=True)
    joined_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return self.full_name


class StudentClassEnrollment(models.Model):
    """
    Link between a student and a class they joined.
    """
    student = models.ForeignKey(Student, on_delete=models.CASCADE, related_name='enrollments')
    classroom = models.ForeignKey(Class, on_delete=models.CASCADE, related_name='enrollments')
    joined_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ('student', 'classroom')

    def clean(self):
        if not self.classroom.active:
            raise ValidationError("This class is not active.")
        
    def __str__(self):
        return f"{self.student.full_name} in {self.classroom.course.name}"


class StudentQuizSubmission(models.Model):
    """
    Represents one student's submission for one quiz.
    """
    student = models.ForeignKey(Student, on_delete=models.CASCADE, related_name='submissions')
    quiz = models.ForeignKey(Quiz, on_delete=models.CASCADE, related_name='submissions')
    submitted_at = models.DateTimeField(auto_now_add=True)
    score = models.DecimalField(max_digits=6, decimal_places=2, blank=True, null=True)
    is_late = models.BooleanField(default=False)

    class Meta:
        unique_together = ('student', 'quiz')

    def __str__(self):
        return f"{self.student.full_name} → {self.quiz.title}"


class StudentAnswer(models.Model):
    """
    Stores an answer submitted by a student for a quiz.
    Uses JSON field to store answer data for different quiz types.
    """
    submission = models.ForeignKey(StudentQuizSubmission, on_delete=models.CASCADE, related_name='answers')
    
    # Flexible answer data stored as JSON
    answer_data = models.JSONField(
        default=dict,
        help_text="Answer data structure varies by quiz type"
    )
    
    # For file uploads (images, drawings) - stored separately for easier handling
    uploaded_file = models.FileField(upload_to='student_uploads/', blank=True, null=True)

    # Auto metadata
    submitted_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        ordering = ['-submitted_at']
        constraints = [
            models.UniqueConstraint(
                fields=['submission'],
                name='unique_answer_per_submission'
            )
        ]

    def clean(self):
        """Basic validation - detailed validation is handled by serializers."""
        super().clean()
        
        # Basic validation only - detailed type-specific validation is in serializers
        if not self.answer_data and not self.uploaded_file:
            raise ValidationError("Answer must have either answer_data or uploaded_file.")

    def get_correct_answer_info(self):
        """Get the correct answer information for this quiz (for reference only)."""
        quiz = self.submission.quiz
        quiz_type_code = quiz.quiz_type
        props = quiz.properties or {}
        
        if quiz_type_code == 'multiple_choice':
            # Return correct choices for reference from properties
            choices = props.get('choices', [])
            correct_choices = [choice for choice in choices if choice.get('is_correct', False)]
            return {
                'type': 'multiple_choice',
                'correct_choices': correct_choices,
                'allow_multiple': bool(props.get('allow_multiple_choices', False))
            }
        elif quiz_type_code == 'short_answer':
            # Return correct answer and keywords for reference from properties
            return {
                'type': 'short_answer',
                'correct_answer': props.get('correct_answer'),
                'expected_keywords': props.get('expected_keywords'),
                'case_sensitive': bool(props.get('case_sensitive', False))
            }
        elif quiz_type_code == 'word_cloud':
            return {
                'type': 'word_cloud',
                'max_words': props.get('max_words_per_student'),
                'allow_duplicates': bool(props.get('allow_duplicates', False))
            }
        elif quiz_type_code in ['drawing', 'image_upload']:
            return {
                'type': quiz_type_code,
                'canvas_width': props.get('canvas_width'),
                'canvas_height': props.get('canvas_height'),
                'max_file_size': props.get('max_file_size_mb'),
                'allowed_formats': props.get('allowed_formats'),
                'drawing_strokes': 'NOT_IMPLEMENTED',
                'drawing_colors': 'NOT_IMPLEMENTED',
                'drawing_shapes': 'NOT_IMPLEMENTED',
                'drawing_metadata': 'NOT_IMPLEMENTED',
                'drawing_analysis': 'NOT_IMPLEMENTED',
                'drawing_comparison': 'NOT_IMPLEMENTED',
                'image_metadata': 'NOT_IMPLEMENTED',
                'image_analysis': 'NOT_IMPLEMENTED',
                'ocr_text': 'NOT_IMPLEMENTED',
                'object_detection': 'NOT_IMPLEMENTED',
                'image_comparison': 'NOT_IMPLEMENTED',
                'file_processing': 'NOT_IMPLEMENTED',
                'content_analysis': 'NOT_IMPLEMENTED',
                'automatic_grading': 'NOT_IMPLEMENTED'
            }
        
        return {'type': 'unknown'}

    def __str__(self):
        return f"Answer by {self.submission.student.full_name} for {self.submission.quiz.title}"
