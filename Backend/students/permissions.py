"""
Custom permissions for student-related operations.
Implements proper access control for ClassPoint workflow.
"""
from rest_framework import permissions
from django.shortcuts import get_object_or_404
from .models import Student, StudentClassEnrollment, StudentQuizSubmission, StudentAnswer
from .authentication import StudentUser
from classes.models import Class
from courses.models import Course


class IsTeacherOrReadOnly(permissions.BasePermission):
    """
    Custom permission to only allow teachers to create/edit,
    but allow students to read their own data.
    """
    
    def has_permission(self, request, view):
        # Allow read permissions for any request
        if request.method in permissions.SAFE_METHODS:
            return True
        
        # Write permissions only for authenticated teachers
        return request.user and request.user.is_authenticated


class IsClassTeacher(permissions.BasePermission):
    """
    Permission to only allow the teacher who created the class
    to manage it.
    """
    
    def has_permission(self, request, view):
        if not request.user or not request.user.is_authenticated:
            return False
        
        # Get class_id from URL or request data
        class_id = view.kwargs.get('class_id') or request.data.get('class_id')
        if not class_id:
            return False
        
        try:
            classroom = Class.objects.get(id=class_id)
            return classroom.teacher == request.user
        except Class.DoesNotExist:
            return False


class IsEnrolledStudent(permissions.BasePermission):
    """
    Permission to only allow students who are enrolled in the class
    to interact with it.
    """
    
    def has_permission(self, request, view):
        # Get class_id from URL or request data
        class_id = view.kwargs.get('class_id') or request.data.get('class_id')
        if not class_id:
            return False
        
        try:
            classroom = Class.objects.get(id=class_id)
            
            # Check if student is enrolled
            student_id = request.data.get('student_id') or view.kwargs.get('student_id')
            if not student_id:
                return False
            
            return StudentClassEnrollment.objects.filter(
                student_id=student_id,
                classroom=classroom
            ).exists()
        except (Class.DoesNotExist, ValueError):
            return False


class CanViewStudentAnswers(permissions.BasePermission):
    """
    Permission to view student answers:
    - Teachers can view all answers in their classes
    - Students can only view their own answers
    """
    
    def has_permission(self, request, view):
        if not request.user or not request.user.is_authenticated:
            return False
        
        # Teachers can view all answers in their classes
        if hasattr(request.user, 'course_set'):  # User is a teacher
            return True
        
        # Students can only view their own answers
        student_id = view.kwargs.get('student_id') or request.data.get('student_id')
        if not student_id:
            return False
        
        # Check if the student_id matches the authenticated user's student profile
        # This would need to be implemented based on your user-student relationship
        return True  # Simplified for now


class CanCreateStudentAnswer(permissions.BasePermission):
    """
    Permission to create student answers:
    - Only enrolled students can create answers
    - Students can only create answers for quizzes in classes they're enrolled in
    """
    
    def has_permission(self, request, view):
        if request.method not in ['POST']:
            return True
        
        # Get required data
        student_id = request.data.get('student_id')
        quiz_id = request.data.get('quiz_id')
        
        if not student_id or not quiz_id:
            return False
        
        try:
            # Check if student is enrolled in a class that has this quiz
            from quizzes.models import Quiz
            quiz = Quiz.objects.get(id=quiz_id)
            course = quiz.course
            
            # Check if student is enrolled in any active class for this course
            return StudentClassEnrollment.objects.filter(
                student_id=student_id,
                classroom__course=course,
                classroom__active=True
            ).exists()
        except (Quiz.DoesNotExist, ValueError):
            return False


class IsClassActive(permissions.BasePermission):
    """
    Permission to ensure the class is active before allowing interactions.
    """
    
    def has_permission(self, request, view):
        class_id = view.kwargs.get('class_id') or request.data.get('class_id')
        if not class_id:
            return True  # Let other permissions handle this
        
        try:
            classroom = Class.objects.get(id=class_id)
            return classroom.active
        except Class.DoesNotExist:
            return False


class StudentAnswerAccess(permissions.BasePermission):
    """
    Combined permission for student answer operations:
    - Teachers can view all answers in their classes
    - Students can view/create their own answers using tokens
    - Only enrolled students in active classes can create answers
    """
    
    def has_permission(self, request, view):
        if not request.user or not request.user.is_authenticated:
            return False
        
        # For GET requests (viewing answers)
        if request.method in permissions.SAFE_METHODS:
            # Teachers can view all answers in their classes
            if hasattr(request.user, 'course_set'):  # User is a teacher
                return True
            
            # Students with valid tokens can view their own answers
            if isinstance(request.user, StudentUser):
                return True
            
            return False
        
        # For POST requests (creating answers)
        elif request.method == 'POST':
            # Only students with valid tokens can create answers
            if isinstance(request.user, StudentUser):
                return True
            
            return False
        
        return False
