"""
Constants and configuration for quiz system.
Centralizes magic numbers and configuration values.
"""

# Quiz Type Codes
class QuizTypeCodes:
    MULTIPLE_CHOICE = 'multiple_choice'
    SHORT_ANSWER = 'short_answer'
    WORD_CLOUD = 'word_cloud'
    DRAWING = 'drawing'
    IMAGE_UPLOAD = 'image_upload'
    # Deprecated types removed per new design

# Quiz Type Names
class QuizTypeNames:
    MULTIPLE_CHOICE = 'Multiple Choice'
    SHORT_ANSWER = 'Short Answer'
    WORD_CLOUD = 'Word Cloud'
    DRAWING = 'Drawing'
    IMAGE_UPLOAD = 'Image Upload'
    # Deprecated types removed per new design

# Validation Limits
class ValidationLimits:
    # Question text limits
    MAX_QUESTION_TEXT_LENGTH = 2000
    MIN_QUESTION_TEXT_LENGTH = 1
    
    # Choice limits
    MIN_CHOICES = 2
    MAX_CHOICES = 10
    MAX_CHOICE_TEXT_LENGTH = 500
    
    # Answer limits
    MAX_CORRECT_ANSWER_LENGTH = 500
    MAX_KEYWORDS_LENGTH = 1000
    
    # File upload limits
    MAX_IMAGE_SIZE_MB = 10
    ALLOWED_IMAGE_FORMATS = ['jpg', 'jpeg', 'png', 'gif', 'webp']
    
    # Drawing canvas limits
    MIN_CANVAS_SIZE = 100
    MAX_CANVAS_SIZE = 2000
    
    # Word cloud limits
    MIN_WORDS_PER_STUDENT = 1
    MAX_WORDS_PER_STUDENT = 10

# Default Values
class DefaultValues:
    # Multiple Choice
    DEFAULT_CHOICE_COUNT = 4
    DEFAULT_POINTS_PER_CORRECT = 1
    DEFAULT_PENALTY_PER_WRONG = 0
    
    # Short Answer
    DEFAULT_MAX_LENGTH = 200
    DEFAULT_CASE_SENSITIVE = False
    
    # Drawing
    DEFAULT_CANVAS_WIDTH = 800
    DEFAULT_CANVAS_HEIGHT = 600
    
    # Image Upload
    DEFAULT_MAX_FILE_SIZE_MB = 5
    DEFAULT_ALLOWED_FORMATS = 'jpg,png,jpeg'
    
    # Word Cloud
    DEFAULT_MAX_WORDS = 3
    
    # Quiz Settings
    DEFAULT_SHOW_TIMER = True
    DEFAULT_ALLOW_LATE_SUBMISSIONS = False
    DEFAULT_SHOW_RESULTS_TO_STUDENTS = True

# Error Messages
class ErrorMessages:
    QUESTION_TEXT_REQUIRED = "Question text is required."
    QUESTION_TEXT_TOO_LONG = f"Question text cannot exceed {ValidationLimits.MAX_QUESTION_TEXT_LENGTH} characters."
    CHOICES_REQUIRED = "Multiple choice questions must have at least one choice."
    MIN_CHOICES_REQUIRED = f"Multiple choice questions must have at least {ValidationLimits.MIN_CHOICES} choices."
    MAX_CHOICES_EXCEEDED = f"Multiple choice questions cannot have more than {ValidationLimits.MAX_CHOICES} choices."
    CHOICE_TEXT_REQUIRED = "Choice text cannot be empty."
    CHOICE_TEXT_TOO_LONG = f"Choice text cannot exceed {ValidationLimits.MAX_CHOICE_TEXT_LENGTH} characters."
    CORRECT_ANSWER_REQUIRED = "At least one choice must be marked as correct."
    CORRECT_ANSWER_TOO_LONG = f"Correct answer cannot exceed {ValidationLimits.MAX_CORRECT_ANSWER_LENGTH} characters."
    KEYWORDS_TOO_LONG = f"Expected keywords cannot exceed {ValidationLimits.MAX_KEYWORDS_LENGTH} characters."
    INVALID_IMAGE_FORMAT = f"Invalid image format. Allowed formats: {', '.join(ValidationLimits.ALLOWED_IMAGE_FORMATS)}"
    IMAGE_TOO_LARGE = f"Image size cannot exceed {ValidationLimits.MAX_IMAGE_SIZE_MB}MB."
    CANVAS_SIZE_INVALID = f"Canvas size must be between {ValidationLimits.MIN_CANVAS_SIZE} and {ValidationLimits.MAX_CANVAS_SIZE} pixels."

# Quiz Configuration
class QuizConfig:
    # Timer settings
    MIN_TIMER_SECONDS = 10
    MAX_TIMER_SECONDS = 3600  # 1 hour
    
    # Auto-close settings
    MIN_AUTO_CLOSE_SECONDS = 30
    MAX_AUTO_CLOSE_SECONDS = 7200  # 2 hours
    
    # Scoring
    MIN_SCORE = 0
    MAX_SCORE = 100
    
    # Competition mode
    COMPETITION_POINTS_MULTIPLIER = 2
    COMPETITION_TIME_BONUS = 0.1  # 10% bonus for speed
