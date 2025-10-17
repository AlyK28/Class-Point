from rest_framework import viewsets, permissions
from .models import Course
from .serializers import CourseSerializer

class CourseViewSet(viewsets.ModelViewSet):
    queryset = Course.objects.all()
    serializer_class = CourseSerializer
    permission_classes = [permissions.IsAuthenticated]

    def perform_create(self, serializer):
        serializer.save(teacher=self.request.user)

    def get_queryset(self):
        """Return only courses owned by the authenticated teacher."""
        user = self.request.user
        if not user or not user.is_authenticated:
            return Course.objects.none()
        return Course.objects.filter(teacher=user).order_by('-created_at')
