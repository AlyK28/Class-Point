from rest_framework import generics, permissions
from .models import Class
from .serializers import ClassSerializer

class ClassCreateView(generics.CreateAPIView):
    """
    Endpoint for teachers to create a class.
    Automatically assigns the logged-in user as the teacher.
    """
    serializer_class = ClassSerializer
    permission_classes = [permissions.IsAuthenticated]

    def perform_create(self, serializer):
        serializer.save(teacher=self.request.user)
