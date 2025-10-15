from django.db import models, transaction, IntegrityError
from django.contrib.auth.models import User
from classes.models import Class
from quizzes.models import Quiz, QuizType
from django.core.exceptions import ValidationError


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
    """
    submission = models.ForeignKey(StudentQuizSubmission, on_delete=models.CASCADE, related_name='answers')

    # For text-based answers
    answer_text = models.TextField(blank=True, null=True)

    # For multiple-choice
    selected_options = models.JSONField(blank=True, null=True, help_text="List of selected option IDs")

    # For drawing/image upload
    uploaded_image = models.ImageField(upload_to='student_uploads/', blank=True, null=True)

    # Auto metadata
    submitted_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return f"Answer by {self.submission.student.full_name} for {self.submission.quiz.title}"
