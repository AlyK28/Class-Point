from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from rest_framework import status, permissions, serializers
from drf_spectacular.utils import extend_schema, OpenApiRequest, OpenApiResponse
from .models import Class
from courses.models import Course
from .serializers import ClassSerializer


class CreateClassFromPowerPointSerializer(serializers.Serializer):
    course_id = serializers.IntegerField(help_text="ID of the Course to create a Class for.")


@extend_schema(
    tags=["Classes"],
    request=CreateClassFromPowerPointSerializer,
    responses={
        201: OpenApiResponse(ClassSerializer, description="Class created successfully"),
        400: OpenApiResponse(description="Missing or invalid course_id"),
        404: OpenApiResponse(description="Course not found"),
    },
    summary="Create a Class from PowerPoint",
    description="Creates a new active Class when a slideshow starts. Requires a valid course_id."
)
@api_view(['POST'])
@permission_classes([permissions.IsAuthenticated])
def create_class_from_powerpoint(request):
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
