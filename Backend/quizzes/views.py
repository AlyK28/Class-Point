from rest_framework import viewsets, permissions
from .models import (
    QuizType, Quiz,
    MultipleChoiceOptions, WordCloudOptions,
    ShortAnswerOptions, DrawingOptions, ImageUploadOptions
)
from .serializers import (
    QuizTypeSerializer, QuizSerializer,
    MultipleChoiceOptionsSerializer, WordCloudOptionsSerializer,
    ShortAnswerOptionsSerializer, DrawingOptionsSerializer, ImageUploadOptionsSerializer
)


# -------- QUIZ TYPES --------
class QuizTypeViewSet(viewsets.ReadOnlyModelViewSet):
    """
    Returns all available quiz types (e.g., Multiple Choice, Word Cloud, etc.)
    """
    queryset = QuizType.objects.filter(is_active=True)
    serializer_class = QuizTypeSerializer
    permission_classes = [permissions.IsAuthenticated]


# -------- QUIZZES --------
class QuizViewSet(viewsets.ModelViewSet):
    """
    CRUD operations for quizzes (includes global options).
    """
    queryset = Quiz.objects.all()
    serializer_class = QuizSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        # Teachers see only their own quizzes
        return Quiz.objects.filter(created_by=self.request.user)

    def perform_create(self, serializer):
        serializer.save(created_by=self.request.user)


# -------- TYPE-SPECIFIC OPTIONS --------
class MultipleChoiceOptionsViewSet(viewsets.ModelViewSet):
    queryset = MultipleChoiceOptions.objects.all()
    serializer_class = MultipleChoiceOptionsSerializer
    permission_classes = [permissions.IsAuthenticated]


class WordCloudOptionsViewSet(viewsets.ModelViewSet):
    queryset = WordCloudOptions.objects.all()
    serializer_class = WordCloudOptionsSerializer
    permission_classes = [permissions.IsAuthenticated]


class ShortAnswerOptionsViewSet(viewsets.ModelViewSet):
    queryset = ShortAnswerOptions.objects.all()
    serializer_class = ShortAnswerOptionsSerializer
    permission_classes = [permissions.IsAuthenticated]


class DrawingOptionsViewSet(viewsets.ModelViewSet):
    queryset = DrawingOptions.objects.all()
    serializer_class = DrawingOptionsSerializer
    permission_classes = [permissions.IsAuthenticated]


class ImageUploadOptionsViewSet(viewsets.ModelViewSet):
    queryset = ImageUploadOptions.objects.all()
    serializer_class = ImageUploadOptionsSerializer
    permission_classes = [permissions.IsAuthenticated]
