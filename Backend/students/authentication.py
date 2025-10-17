"""
Student authentication system for ClassPoint.
Students get temporary tokens when they join a class.
"""
import jwt
import uuid
from datetime import timedelta
from django.utils import timezone
from django.conf import settings
from django.core.exceptions import ValidationError
from rest_framework.authentication import BaseAuthentication
from rest_framework.exceptions import AuthenticationFailed
from .models import Student, StudentClassEnrollment


class StudentToken:
    """Simple token system for students."""
    
    @staticmethod
    def generate_token(student_id, class_id, enrollment_id):
        """Generate a JWT token for a student in a specific class."""
        payload = {
            'student_id': student_id,
            'class_id': class_id,
            'enrollment_id': enrollment_id,
            'token_type': 'student',
            'exp': timezone.now() + timedelta(hours=24),  # 24 hour expiry
            'iat': timezone.now(),
            'jti': str(uuid.uuid4())  # Unique token ID
        }
        
        # Use Django's SECRET_KEY for signing
        token = jwt.encode(payload, settings.SECRET_KEY, algorithm='HS256')
        return token
    
    @staticmethod
    def decode_token(token):
        """Decode and validate a student token."""
        try:
            payload = jwt.decode(token, settings.SECRET_KEY, algorithms=['HS256'])
            
            # Validate token type
            if payload.get('token_type') != 'student':
                raise AuthenticationFailed('Invalid token type')
            
            # Check if enrollment still exists and is valid
            enrollment_id = payload.get('enrollment_id')
            if not enrollment_id:
                raise AuthenticationFailed('Invalid token')
            
            enrollment = StudentClassEnrollment.objects.select_related(
                'student', 'classroom'
            ).get(id=enrollment_id)
            
            # Check if class is still active
            if not enrollment.classroom.active:
                raise AuthenticationFailed('Class is no longer active')
            
            return {
                'student_id': payload['student_id'],
                'class_id': payload['class_id'],
                'enrollment_id': payload['enrollment_id'],
                'student': enrollment.student,
                'classroom': enrollment.classroom,
                'enrollment': enrollment
            }
            
        except jwt.ExpiredSignatureError:
            raise AuthenticationFailed('Token has expired')
        except jwt.InvalidTokenError:
            raise AuthenticationFailed('Invalid token')
        except StudentClassEnrollment.DoesNotExist:
            raise AuthenticationFailed('Invalid enrollment')


class StudentAuthentication(BaseAuthentication):
    """
    Custom authentication for students using JWT tokens.
    Students authenticate with tokens received when joining a class.
    """
    
    def authenticate(self, request):
        """Authenticate student using token from header."""
        auth_header = request.META.get('HTTP_AUTHORIZATION')
        
        if not auth_header or not auth_header.startswith('Bearer '):
            return None
        
        try:
            # Extract token from "Bearer <token>" format
            token = auth_header.split(' ')[1]
            
            # Quick check: if token doesn't contain student info, skip
            # This prevents JWT from trying to validate student tokens
            import jwt
            try:
                # Decode without verification to check token type
                unverified_payload = jwt.decode(token, options={"verify_signature": False})
                if unverified_payload.get('token_type') != 'student':
                    return None
            except:
                return None
            
            # Now properly validate the student token
            token_data = StudentToken.decode_token(token)
            
            # Create a proper user object for students
            student_user = StudentUser(
                student=token_data['student'],
                classroom=token_data['classroom'],
                enrollment=token_data['enrollment']
            )
            
            # Return (user, auth) tuple
            return (student_user, token)
            
        except (IndexError, AuthenticationFailed, Exception):
            return None
    
    def authenticate_header(self, request):
        """Return the authentication header."""
        return 'Bearer'


class StudentUser:
    """
    Simple user object for students.
    Represents an authenticated student in a specific class.
    """
    
    def __init__(self, student, classroom, enrollment):
        self.student = student
        self.classroom = classroom
        self.enrollment = enrollment
        self.id = student.id
        self.is_authenticated = True
        self.is_anonymous = False
    
    def __str__(self):
        return f"Student({self.student.full_name})"
    
    def has_perm(self, perm, obj=None):
        """Students have limited permissions."""
        return False
    
    def has_module_perms(self, app_label):
        """Students don't have module permissions."""
        return False
