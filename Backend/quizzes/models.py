# quizzes/models.py
from django.db import models
from django.core.exceptions import ValidationError
from courses.models import Course
from django.contrib.auth.models import User
from .helpers import QuizTypeValidator
from .constants import ValidationLimits, ErrorMessages, QuizTypeCodes, QuizTypeNames


class Quiz(models.Model):
    course = models.ForeignKey(Course, on_delete=models.CASCADE, related_name='quizzes')
    title = models.CharField(max_length=255)
    quiz_type = models.CharField(
        max_length=50,
        choices=[
            (QuizTypeCodes.MULTIPLE_CHOICE, QuizTypeNames.MULTIPLE_CHOICE),
            (QuizTypeCodes.SHORT_ANSWER, QuizTypeNames.SHORT_ANSWER),
            (QuizTypeCodes.WORD_CLOUD, QuizTypeNames.WORD_CLOUD),
            (QuizTypeCodes.DRAWING, QuizTypeNames.DRAWING),
            (QuizTypeCodes.IMAGE_UPLOAD, QuizTypeNames.IMAGE_UPLOAD),
        ],
    )
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

    # Type-specific properties stored as flexible JSON
    properties = models.JSONField(default=dict)

    class Meta:
        ordering = ['-created_at']

    def clean(self):
        """Validate quiz based on its type and properties."""
        # Basic question text presence per type
        props = self.properties or {}

        # Shared question_text rules where applicable
        question_text = props.get('question_text', '')
        if self.quiz_type in [QuizTypeCodes.MULTIPLE_CHOICE, QuizTypeCodes.SHORT_ANSWER, QuizTypeCodes.WORD_CLOUD, QuizTypeCodes.DRAWING, QuizTypeCodes.IMAGE_UPLOAD]:
            if not question_text or not str(question_text).strip():
                raise ValidationError(ErrorMessages.QUESTION_TEXT_REQUIRED)
            if len(str(question_text)) > ValidationLimits.MAX_QUESTION_TEXT_LENGTH:
                raise ValidationError(ErrorMessages.QUESTION_TEXT_TOO_LONG)

        if self.quiz_type == QuizTypeCodes.MULTIPLE_CHOICE:
            choices = props.get('choices', [])
            # Delegate to helper for structure/content validation
            QuizTypeValidator.validate_multiple_choice_options(choices)

        elif self.quiz_type == QuizTypeCodes.SHORT_ANSWER:
            correct_answer = props.get('correct_answer')
            expected_keywords = props.get('expected_keywords')
            # Reuse validator constraints
            # Note: expected_keywords can be comma-separated string
            from .helpers import QuizTypeValidator as _V
            _V.validate_short_answer_content(question_text, correct_answer, expected_keywords)
            # Ensure at least one grading basis exists
            if not (correct_answer or (expected_keywords and str(expected_keywords).strip())):
                raise ValidationError("Either correct answer or expected keywords must be provided for grading.")

        elif self.quiz_type == QuizTypeCodes.WORD_CLOUD:
            max_words_per_student = int(props.get('max_words_per_student', 1))
            if max_words_per_student < ValidationLimits.MIN_WORDS_PER_STUDENT:
                raise ValidationError(
                    f"Maximum words per student must be at least {ValidationLimits.MIN_WORDS_PER_STUDENT}"
                )
            if max_words_per_student > ValidationLimits.MAX_WORDS_PER_STUDENT:
                raise ValidationError(
                    f"Maximum words per student cannot exceed {ValidationLimits.MAX_WORDS_PER_STUDENT}"
                )

        elif self.quiz_type == QuizTypeCodes.DRAWING:
            # Basic validation for drawing quiz properties
            canvas_width = props.get('canvas_width', 800)
            canvas_height = props.get('canvas_height', 600)
            if not isinstance(canvas_width, int) or canvas_width <= 0:
                raise ValidationError("Canvas width must be a positive integer")
            if not isinstance(canvas_height, int) or canvas_height <= 0:
                raise ValidationError("Canvas height must be a positive integer")

        elif self.quiz_type == QuizTypeCodes.IMAGE_UPLOAD:
            # Basic validation for image upload quiz properties
            max_file_size = props.get('max_file_size_mb', 5)
            allowed_formats = props.get('allowed_formats', 'jpg,png,jpeg')
            if not isinstance(max_file_size, int) or max_file_size <= 0:
                raise ValidationError("Max file size must be a positive integer")
            if not isinstance(allowed_formats, str) or not allowed_formats.strip():
                raise ValidationError("Allowed formats must be a non-empty string")

    def __str__(self):
        return f"{self.title} ({self.quiz_type})"
