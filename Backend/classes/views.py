from rest_framework import viewsets, permissions, serializers
from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from rest_framework import status
from .models import Class
from courses.models import Course
from .serializers import ClassSerializer


class CreateClassFromPowerPointSerializer(serializers.Serializer):
    course_id = serializers.IntegerField(help_text="ID of the Course to create a Class for.")


class EndClassSerializer(serializers.Serializer):
    active = serializers.BooleanField(help_text="Set to false to end the class session.")


class ClassViewSet(viewsets.ReadOnlyModelViewSet):
    """
    ViewSet for listing and retrieving classes.
    Teachers can only see their own classes.
    """
    serializer_class = ClassSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Only show classes created by the authenticated teacher."""
        return Class.objects.filter(teacher=self.request.user).order_by('-created_at')


@api_view(['POST'])
@permission_classes([permissions.IsAuthenticated])
def create_class_from_powerpoint(request):
    """
    Create a new class session from a course.
    
    Creates a new active class when a teacher starts presenting.
    Requires a valid course_id in the request data.
    
    Returns:
        - 201: Class created successfully with join code
        - 400: Missing or invalid course_id
        - 404: Course not found
    """
    course_id = request.data.get('course_id') or request.query_params.get('course_id')

    if not course_id:
        return Response(
            {"detail": "course_id is required."},
            status=status.HTTP_400_BAD_REQUEST
        )

    try:
        course = Course.objects.get(id=course_id)
    except Course.DoesNotExist:
        return Response(
            {"detail": f"Course with id={course_id} not found."},
            status=status.HTTP_404_NOT_FOUND
        )

    new_class = Class.objects.create(course=course, teacher=request.user, active=True)
    serializer = ClassSerializer(new_class)
    return Response(serializer.data, status=status.HTTP_201_CREATED)


@api_view(['PATCH'])
@permission_classes([permissions.IsAuthenticated])
def end_class(request, class_id):
    """
    End a class session by setting active=False.
    
    Only the teacher who created the class can end it.
    
    Args:
        class_id: ID of the class to end
        
    Returns:
        - 200: Class ended successfully
        - 400: Invalid request data
        - 404: Class not found
        - 403: Not authorized to modify this class
    """
    try:
        class_obj = Class.objects.get(id=class_id)
    except Class.DoesNotExist:
        return Response(
            {"detail": f"Class with id={class_id} not found."},
            status=status.HTTP_404_NOT_FOUND
        )

    # Check if the authenticated user is the teacher who created this class
    if class_obj.teacher != request.user:
        return Response(
            {"detail": "You are not authorized to modify this class."},
            status=status.HTTP_403_FORBIDDEN
        )

    # Update the class to inactive
    class_obj.active = False
    class_obj.save()

    serializer = ClassSerializer(class_obj)
    return Response(serializer.data, status=status.HTTP_200_OK)
