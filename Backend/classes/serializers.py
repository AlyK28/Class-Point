from rest_framework import serializers
from .models import Class
from courses.models import Course

class ClassSerializer(serializers.ModelSerializer):
    course_id = serializers.PrimaryKeyRelatedField(
        queryset=Course.objects.all(), write_only=True, source='course'
    )
    course_name = serializers.CharField(source='course.name', read_only=True)
    teacher_name = serializers.CharField(source='teacher.username', read_only=True)

    class Meta:
        model = Class
        fields = [
            'id',
            'code',
            'active',
            'created_at',
            'course_id',
            'course_name',
            'teacher_name',
        ]
        read_only_fields = ['id', 'code', 'created_at', 'teacher_name', 'course_name']
