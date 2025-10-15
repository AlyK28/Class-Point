from rest_framework.decorators import api_view, permission_classes
from rest_framework.permissions import IsAuthenticated
from rest_framework.response import Response
from rest_framework import status
from courses.models import Course
from .models import Class


@api_view(["POST"])
@permission_classes([IsAuthenticated])
def create_class_from_powerpoint(request):
    """
    Create or find a Course based on PowerPoint name, then start a new Class.
    """
    ppt_name = request.data.get("powerpoint_name")
    if not ppt_name:
        return Response({"error": "Missing PowerPoint name"}, status=400)

    course_name = ppt_name.rsplit('.', 1)[0]
    course, _ = Course.objects.get_or_create(name=course_name, teacher=request.user)

    new_class = Class.objects.create(
        name=f"{course_name} - Session",
        teacher=request.user,
        course=course
    )

    return Response({
        "course": course.name,
        "class_code": new_class.code,
        "message": "Class created successfully"
    }, status=status.HTTP_201_CREATED)
