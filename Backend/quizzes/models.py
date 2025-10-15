# quizzes/models.py
from django.db import models
from django.core.exceptions import ValidationError
from courses.models import Course
from django.contrib.auth.models import User


class QuizType(models.Model):
    code = models.CharField(max_length=50, unique=True)   # e.g. "multiple_choice"
    name = models.CharField(max_length=100)               # e.g. "Multiple Choice"
    description = models.TextField(blank=True, null=True)
    is_active = models.BooleanField(default=True)
    has_type_options = models.BooleanField(default=False) # True if it uses a type-options record

    def __str__(self):
        return self.name


class Quiz(models.Model):
    course = models.ForeignKey(Course, on_delete=models.CASCADE, related_name='quizzes')
    title = models.CharField(max_length=255)
    quiz_type = models.ForeignKey(QuizType, on_delete=models.PROTECT, related_name='quizzes')
    created_by = models.ForeignKey(User, on_delete=models.PROTECT, related_name='created_quizzes')
    created_at = models.DateTimeField(auto_now_add=True)

    # --- GLOBAL OPTIONS (apply to all quiz types) ---
    start_with_slide = models.BooleanField(default=True)
    minimize_results_window_on_start = models.BooleanField(default=False)
    auto_close_after_seconds = models.PositiveIntegerField(
        blank=True, null=True,
        help_text="If set, closes submissions after N seconds from start."
    )
    show_timer = models.BooleanField(default=True)
    allow_late_submissions = models.BooleanField(default=False)
    show_results_to_students = models.BooleanField(default=True)

    class Meta:
        ordering = ['-created_at']

    def clean(self):
        # If the type requires a type-options object, ensure it exists
        if self.quiz_type and self.quiz_type.has_type_options:
            has_opts = (
                hasattr(self, 'mcq_options') or
                hasattr(self, 'wordcloud_options') or
                hasattr(self, 'shortanswer_options') or
                hasattr(self, 'drawing_options') or
                hasattr(self, 'imageupload_options')
            )
            if not has_opts:
                raise ValidationError("Type-specific options object is required for this quiz type.")

    def __str__(self):
        return f"{self.title} ({self.quiz_type.name})"


# -------- TYPE-SPECIFIC OPTIONS (OneToOne per quiz) --------
class MultipleChoiceOptions(models.Model):
    quiz = models.OneToOneField(Quiz, on_delete=models.CASCADE, related_name='mcq_options')

    # Specific controls
    number_of_choices = models.PositiveIntegerField(default=4)
    allow_multiple_choices = models.BooleanField(default=False)
    has_correct_answer = models.BooleanField(default=True)

    # Competition settings
    competition_mode = models.BooleanField(default=False)
    randomize_choice_order = models.BooleanField(default=True)
    points_per_correct = models.PositiveIntegerField(default=1)
    penalty_per_wrong = models.IntegerField(default=0)  # allow negative

    def __str__(self):
        return f"MCQ Options for {self.quiz.title}"


class WordCloudOptions(models.Model):
    quiz = models.OneToOneField(Quiz, on_delete=models.CASCADE, related_name='wordcloud_options')
    max_words_per_student = models.PositiveIntegerField(default=3)
    allow_duplicates = models.BooleanField(default=False)
    normalize_case = models.BooleanField(default=True)

    def __str__(self):
        return f"Word Cloud Options for {self.quiz.title}"


class ShortAnswerOptions(models.Model):
    quiz = models.OneToOneField(Quiz, on_delete=models.CASCADE, related_name='shortanswer_options')
    max_length = models.PositiveIntegerField(default=200)
    expected_keywords = models.TextField(blank=True, help_text="Comma-separated keywords for rubric")
    case_sensitive = models.BooleanField(default=False)
    use_regex = models.BooleanField(default=False)

    def __str__(self):
        return f"Short Answer Options for {self.quiz.title}"


class DrawingOptions(models.Model):
    quiz = models.OneToOneField(Quiz, on_delete=models.CASCADE, related_name='drawing_options')
    canvas_width = models.PositiveIntegerField(default=800)
    canvas_height = models.PositiveIntegerField(default=600)
    allow_colors = models.BooleanField(default=True)
    allow_eraser = models.BooleanField(default=True)

    def __str__(self):
        return f"Drawing Options for {self.quiz.title}"


class ImageUploadOptions(models.Model):
    quiz = models.OneToOneField(Quiz, on_delete=models.CASCADE, related_name='imageupload_options')
    max_file_size_mb = models.PositiveIntegerField(default=5)
    allowed_formats = models.CharField(max_length=100, default='jpg,png,jpeg')
    allow_multiple_files = models.BooleanField(default=False)

    def __str__(self):
        return f"Image Upload Options for {self.quiz.title}"
