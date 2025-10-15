from rest_framework import serializers
from .models import Student, StudentClassEnrollment, StudentQuizSubmission, StudentAnswer
from classes.models import Class
from quizzes.models import Quiz


class StudentSerializer(serializers.ModelSerializer):
    class Meta:
        model = Student
        fields = ['id', 'full_name', 'email', 'joined_at']


class ClassSimpleSerializer(serializers.ModelSerializer):
    """Lightweight serializer to represent a joined class."""
    course_name = serializers.CharField(source='course.name', read_only=True)
    teacher_name = serializers.CharField(source='teacher.username', read_only=True)

    class Meta:
        model = Class
        fields = ['id', 'code', 'course_name', 'teacher_name', 'active']  # ✅ removed 'name'


class StudentClassEnrollmentSerializer(serializers.ModelSerializer):
    student = StudentSerializer(read_only=True)
    classroom = ClassSimpleSerializer(read_only=True)
    classroom_id = serializers.PrimaryKeyRelatedField(
        queryset=Class.objects.all(), write_only=True, source='classroom'
    )

    class Meta:
        model = StudentClassEnrollment
        fields = ['id', 'student', 'classroom', 'classroom_id', 'joined_at']


class StudentQuizSubmissionSerializer(serializers.ModelSerializer):
    quiz_title = serializers.CharField(source='quiz.title', read_only=True)
    quiz_type = serializers.CharField(source='quiz.quiz_type', read_only=True)

    class Meta:
        model = StudentQuizSubmission
        fields = [
            'id',
            'student',
            'quiz',
            'quiz_title',
            'quiz_type',
            'score',
            'is_late',
            'submitted_at',
        ]


class StudentAnswerSerializer(serializers.ModelSerializer):
    quiz_id = serializers.IntegerField(source='submission.quiz.id', read_only=True)
    quiz_title = serializers.CharField(source='submission.quiz.title', read_only=True)

    class Meta:
        model = StudentAnswer
        fields = [
            'id',
            'submission',
            'quiz_id',
            'quiz_title',
            'answer_text',
            'selected_options',
            'uploaded_image',
            'submitted_at',
        ]
