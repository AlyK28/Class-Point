from rest_framework import serializers
from django.db import transaction
from .models import Quiz
from .constants import QuizTypeCodes
import uuid


class QuizSerializer(serializers.ModelSerializer):
    properties = serializers.DictField(required=True)

    class Meta:
        model = Quiz
        fields = [
            'id', 'course', 'title', 'quiz_type',
            'created_by', 'created_at',
            'start_with_slide', 'minimize_results_window_on_start',
            'auto_close_after_seconds', 'show_timer',
            'allow_late_submissions', 'show_results_to_students',
            'properties', 'multi_question_id', 'question_order'
        ]
        read_only_fields = ['created_by', 'created_at']

    def create(self, validated_data):
        request = self.context.get('request')
        if request and request.user and not validated_data.get('created_by'):
            validated_data['created_by'] = request.user

        with transaction.atomic():
            quiz = Quiz.objects.create(**validated_data)
        return quiz


# Multi-Quiz Serializers
class QuestionDataSerializer(serializers.Serializer):
    """Serializer for individual question data within a multi-quiz"""
    title = serializers.CharField()
    quiz_type = serializers.CharField()
    properties = serializers.DictField()
    question_order = serializers.IntegerField()


class MultiQuizSerializer(serializers.Serializer):
    """Serializer for creating entire multi-quiz"""
    title = serializers.CharField()
    course = serializers.IntegerField()
    questions = serializers.ListField(child=QuestionDataSerializer())
    
    def validate_course(self, value):
        """Validate that user is teacher of this course"""
        from courses.models import Course
        try:
            course = Course.objects.get(id=value)
            # The course ownership check will be done in the view
            return value
        except Course.DoesNotExist:
            raise serializers.ValidationError("Course does not exist")
    
    def create(self, validated_data):
        """Create multiple Quiz objects with same multi_question_id"""
        request = self.context.get('request')
        if not request or not request.user:
            raise serializers.ValidationError("User must be authenticated")
        
        # Generate unique multi_question_id
        multi_question_id = uuid.uuid4()
        
        # Get course
        from courses.models import Course
        course = Course.objects.get(id=validated_data['course'])
        
        # Create Quiz objects for each question
        created_quizzes = []
        for question_data in validated_data['questions']:
            quiz = Quiz.objects.create(
                course=course,
                title=question_data['title'],
                quiz_type=question_data['quiz_type'],
                properties=question_data['properties'],
                multi_question_id=multi_question_id,
                question_order=question_data['question_order'],
                created_by=request.user
            )
            created_quizzes.append(quiz)
        
        return {
            'multi_question_id': multi_question_id,
            'title': validated_data['title'],
            'course': course.id,
            'questions': created_quizzes
        }


class MultiQuizListSerializer(serializers.Serializer):
    """Serializer for listing multi-quiz"""
    multi_question_id = serializers.UUIDField()
    title = serializers.CharField()
    question_count = serializers.IntegerField()
    created_at = serializers.DateTimeField()