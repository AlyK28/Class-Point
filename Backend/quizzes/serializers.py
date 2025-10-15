from rest_framework import serializers
from .models import (
    QuizType, Quiz,
    MultipleChoiceOptions, WordCloudOptions,
    ShortAnswerOptions, DrawingOptions, ImageUploadOptions
)


# -------- QUIZ TYPE SERIALIZER --------
class QuizTypeSerializer(serializers.ModelSerializer):
    class Meta:
        model = QuizType
        fields = ['id', 'code', 'name', 'description', 'is_active', 'has_type_options']


# -------- TYPE-SPECIFIC OPTION SERIALIZERS --------
class MultipleChoiceOptionsSerializer(serializers.ModelSerializer):
    class Meta:
        model = MultipleChoiceOptions
        fields = [
            'id', 'number_of_choices', 'allow_multiple_choices',
            'has_correct_answer', 'competition_mode',
            'randomize_choice_order', 'points_per_correct', 'penalty_per_wrong'
        ]


class WordCloudOptionsSerializer(serializers.ModelSerializer):
    class Meta:
        model = WordCloudOptions
        fields = ['id', 'max_words_per_student', 'allow_duplicates', 'normalize_case']


class ShortAnswerOptionsSerializer(serializers.ModelSerializer):
    class Meta:
        model = ShortAnswerOptions
        fields = ['id', 'max_length', 'expected_keywords', 'case_sensitive', 'use_regex']


class DrawingOptionsSerializer(serializers.ModelSerializer):
    class Meta:
        model = DrawingOptions
        fields = ['id', 'canvas_width', 'canvas_height', 'allow_colors', 'allow_eraser']


class ImageUploadOptionsSerializer(serializers.ModelSerializer):
    class Meta:
        model = ImageUploadOptions
        fields = ['id', 'max_file_size_mb', 'allowed_formats', 'allow_multiple_files']


# -------- QUIZ SERIALIZER (MAIN) --------
class QuizSerializer(serializers.ModelSerializer):
    quiz_type_display = serializers.CharField(source='quiz_type.name', read_only=True)

    # Nested read-only type-specific options
    mcq_options = MultipleChoiceOptionsSerializer(read_only=True)
    wordcloud_options = WordCloudOptionsSerializer(read_only=True)
    shortanswer_options = ShortAnswerOptionsSerializer(read_only=True)
    drawing_options = DrawingOptionsSerializer(read_only=True)
    imageupload_options = ImageUploadOptionsSerializer(read_only=True)

    class Meta:
        model = Quiz
        fields = [
            'id', 'course', 'title', 'quiz_type', 'quiz_type_display',
            'created_by', 'created_at',

            # global options
            'start_with_slide', 'minimize_results_window_on_start',
            'auto_close_after_seconds', 'show_timer',
            'allow_late_submissions', 'show_results_to_students',

            # nested options
            'mcq_options', 'wordcloud_options', 'shortanswer_options',
            'drawing_options', 'imageupload_options'
        ]
        read_only_fields = ['created_by', 'created_at']

    def create(self, validated_data):
        request = self.context.get('request')
        quiz = Quiz.objects.create(created_by=request.user, **validated_data)
        return quiz
