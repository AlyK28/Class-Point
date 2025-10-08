from rest_framework import serializers
from .models import Class

class ClassSerializer(serializers.ModelSerializer):
    teacher = serializers.ReadOnlyField(source='teacher.username')

    class Meta:
        model = Class
        fields = ['id', 'name','code', 'teacher', 'active', 'created_at']
        read_only_fields = ['code', 'teacher', 'created_at']
