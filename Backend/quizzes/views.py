from rest_framework import viewsets, permissions, status
from rest_framework.views import APIView
from rest_framework.response import Response
from django.db import transaction
from .models import Quiz
from .serializers import QuizSerializer
from .constants import QuizTypeCodes


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


# -------- CREATE ENDPOINTS PER TYPE (single call) --------
class CreateShortAnswerQuizView(APIView):
    permission_classes = [permissions.IsAuthenticated]

    @transaction.atomic
    def post(self, request):
        """
        Create a Short Answer quiz.
        Expected body: { course, title, properties: { question_text, correct_answer, expected_keywords, case_sensitive, max_length, use_regex } }
        """
        course = request.data.get('course')
        title = request.data.get('title')
        properties = request.data.get('properties', {})

        serializer = QuizSerializer(data={
            'course': course,
            'title': title,
            'quiz_type': QuizTypeCodes.SHORT_ANSWER,
            'properties': properties
        }, context={'request': request})
        serializer.is_valid(raise_exception=True)
        quiz = serializer.save()
        return Response(serializer.data, status=status.HTTP_201_CREATED)


class CreateMultipleChoiceQuizView(APIView):
    permission_classes = [permissions.IsAuthenticated]

    @transaction.atomic
    def post(self, request):
        """
        Create a Multiple Choice quiz.
        Expected body: { course, title, properties: { question_text, choices[], allow_multiple_choices, number_of_choices, has_correct_answer, competition_mode, randomize_choice_order, points_per_correct, penalty_per_wrong } }
        """
        course = request.data.get('course')
        title = request.data.get('title')
        properties = request.data.get('properties', {})

        serializer = QuizSerializer(data={
            'course': course,
            'title': title,
            'quiz_type': QuizTypeCodes.MULTIPLE_CHOICE,
            'properties': properties
        }, context={'request': request})
        serializer.is_valid(raise_exception=True)
        quiz = serializer.save()
        return Response(serializer.data, status=status.HTTP_201_CREATED)


class CreateWordCloudQuizView(APIView):
    permission_classes = [permissions.IsAuthenticated]

    @transaction.atomic
    def post(self, request):
        """
        Create a Word Cloud quiz.
        Expected body: { course, title, properties: { question_text, max_words_per_student, allow_duplicates, normalize_case } }
        """
        course = request.data.get('course')
        title = request.data.get('title')
        properties = request.data.get('properties', {})

        serializer = QuizSerializer(data={
            'course': course,
            'title': title,
            'quiz_type': QuizTypeCodes.WORD_CLOUD,
            'properties': properties
        }, context={'request': request})
        serializer.is_valid(raise_exception=True)
        quiz = serializer.save()
        return Response(serializer.data, status=status.HTTP_201_CREATED)
