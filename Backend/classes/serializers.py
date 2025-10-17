from rest_framework import serializers
from .models import Class
from courses.models import Course
from students.models import StudentClassEnrollment

class ClassSerializer(serializers.ModelSerializer):
    course_id = serializers.PrimaryKeyRelatedField(
        queryset=Course.objects.all(), write_only=True, source='course'
    )
    course_name = serializers.CharField(source='course.name', read_only=True)
    teacher_name = serializers.CharField(source='teacher.username', read_only=True)
    enrollment_count = serializers.SerializerMethodField()
    student_count = serializers.SerializerMethodField()

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
            'enrollment_count',
            'student_count',
        ]
        read_only_fields = ['id', 'code', 'created_at', 'teacher_name', 'course_name', 'enrollment_count', 'student_count']

    def get_enrollment_count(self, obj):
        """Get the number of students enrolled in this class."""
        return StudentClassEnrollment.objects.filter(classroom=obj).count()

    def get_student_count(self, obj):
        """Get the number of unique students enrolled in this class."""
        return StudentClassEnrollment.objects.filter(classroom=obj).values('student').distinct().count()
    
    def validate(self, data):
        """Validate that teacher can't have more than one active class per course."""
        teacher = self.context['request'].user
        course = data.get('course')
        active = data.get('active', True)
        
        if active and course:
            # Check if there's already an active class for this teacher and course
            existing_active = Class.objects.filter(
                teacher=teacher,
                course=course,
                active=True
            )
            
            # If updating an existing instance, exclude it from the check
            if self.instance:
                existing_active = existing_active.exclude(pk=self.instance.pk)
            
            if existing_active.exists():
                existing_class = existing_active.first()
                raise serializers.ValidationError(
                    f"You already have an active class for course '{course.name}' (Code: {existing_class.code}). "
                    "Only one active class per course is allowed. Please deactivate the existing class first."
                )
        
        return data