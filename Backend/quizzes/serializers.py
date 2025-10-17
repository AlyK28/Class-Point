from rest_framework import serializers
from django.db import transaction
from .models import Quiz
from .constants import QuizTypeCodes


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
            'properties'
        ]
        read_only_fields = ['created_by', 'created_at']

    def create(self, validated_data):
        request = self.context.get('request')
        if request and request.user and not validated_data.get('created_by'):
            validated_data['created_by'] = request.user

        with transaction.atomic():
            quiz = Quiz.objects.create(**validated_data)
        return quiz
