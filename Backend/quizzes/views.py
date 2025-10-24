from rest_framework import viewsets, permissions, status
from rest_framework.views import APIView
from rest_framework.response import Response
from django.db import transaction
from django.db.models import Count
from .models import Quiz
from .serializers import QuizSerializer, MultiQuizSerializer, MultiQuizListSerializer
from .constants import QuizTypeCodes
import uuid


# -------- QUIZZES --------
class QuizViewSet(viewsets.ModelViewSet):
    """
    CRUD operations for quizzes (includes global options).
    """
    queryset = Quiz.objects.all()
    serializer_class = QuizSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        # For list: only standalone quizzes
        # For update/delete: include all quizzes (including multi-quiz questions)
        if self.action in ['list']:
            return Quiz.objects.filter(
                created_by=self.request.user,
                multi_question_id__isnull=True  # Only standalone quizzes
            )
        # For retrieve, update, delete: include all quizzes
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


# Multi-Quiz Views
class MultiQuizViewSet(viewsets.ViewSet):
    """ViewSet for managing multi-quiz operations"""
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """Get all multi-quiz created by the current user"""
        return Quiz.objects.filter(
            created_by=self.request.user,
            multi_question_id__isnull=False
        )
    
    def list(self, request):
        """List all multi-quiz grouped by multi_question_id"""
        # Get all unique multi_question_ids for this teacher
        multi_quiz_ids = Quiz.objects.filter(
            created_by=request.user,
            multi_question_id__isnull=False
        ).values_list('multi_question_id', flat=True).distinct()
        
        # Build response with quizzes grouped by multi_question_id
        result = {}
        for multi_id in multi_quiz_ids:
            quizzes = Quiz.objects.filter(multi_question_id=multi_id).order_by('question_order')
            if quizzes.exists():
                quiz_serializer = QuizSerializer(quizzes, many=True)
                result[str(multi_id)] = quiz_serializer.data
        
        return Response(result)
    
    def create(self, request):
        """Create a new multi-quiz with multiple questions"""
        serializer = MultiQuizSerializer(data=request.data, context={'request': request})
        serializer.is_valid(raise_exception=True)
        
        result = serializer.save()
        
        # Return the created quizzes
        quiz_serializer = QuizSerializer(result['questions'], many=True)
        return Response({
            'multi_question_id': result['multi_question_id'],
            'title': result['title'],
            'course': result['course'],
            'questions': quiz_serializer.data
        }, status=status.HTTP_201_CREATED)
    
    def retrieve(self, request, pk=None):
        """Get a specific multi-quiz with all its questions"""
        try:
            quizzes = Quiz.objects.filter(
                multi_question_id=pk,
                created_by=request.user
            ).order_by('question_order')
            
            if not quizzes.exists():
                return Response(
                    {'detail': 'Multi-quiz not found'}, 
                    status=status.HTTP_404_NOT_FOUND
                )
            
            serializer = QuizSerializer(quizzes, many=True)
            return Response(serializer.data)
        except Exception as e:
            return Response(
                {'detail': str(e)}, 
                status=status.HTTP_400_BAD_REQUEST
            )
    
    def destroy(self, request, pk=None):
        """Delete an entire multi-quiz"""
        try:
            quizzes = Quiz.objects.filter(
                multi_question_id=pk,
                created_by=request.user
            )
            
            if not quizzes.exists():
                return Response(
                    {'detail': 'Multi-quiz not found'}, 
                    status=status.HTTP_404_NOT_FOUND
                )
            
            quizzes.delete()
            return Response(status=status.HTTP_204_NO_CONTENT)
        except Exception as e:
            return Response(
                {'detail': str(e)}, 
                status=status.HTTP_400_BAD_REQUEST
            )

