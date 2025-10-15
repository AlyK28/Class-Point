from rest_framework import serializers
from .models import Course

class CourseSerializer(serializers.ModelSerializer):
    class Meta:
        model = Course
        fields = ['id', 'name', 'teacher', 'created_at']
        read_only_fields = ['id', 'teacher', 'created_at']
