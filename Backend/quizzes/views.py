from rest_framework import viewsets, permissions, status
from rest_framework.views import APIView
from rest_framework.response import Response
from django.db import transaction
from .models import Quiz
from .serializers import QuizSerializer
from .constants import QuizTypeCodes

# ALAA_SAJA_TODO: Import uuid for generating multi_question_id for multi-quiz
# Add: import uuid


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


class QuizSubmissionStatsView(APIView):
    """
    Get submission statistics for a multiple choice quiz.
    Returns the count of submissions for each choice.
    """
    permission_classes = [permissions.IsAuthenticated]

    def get(self, request, quiz_id):
        try:
            quiz = Quiz.objects.get(id=quiz_id, created_by=request.user)
        except Quiz.DoesNotExist:
            return Response({'error': 'Quiz not found or you do not have permission to view it.'}, 
                          status=status.HTTP_404_NOT_FOUND)

        if quiz.quiz_type != QuizTypeCodes.MULTIPLE_CHOICE:
            return Response({'error': 'This endpoint only supports multiple choice quizzes.'}, 
                          status=status.HTTP_400_BAD_REQUEST)

        # Get all submissions for this quiz
        from students.models import StudentQuizSubmission, StudentAnswer
        submissions = StudentQuizSubmission.objects.filter(quiz=quiz).select_related('student')
        
        # Get properties
        properties = quiz.properties or {}
        choices = properties.get('choices', [])
        number_of_choices = properties.get('number_of_choices', len(choices))
        
        # Initialize counts and student lists for each choice
        choice_counts = [0] * number_of_choices
        choice_students = [[] for _ in range(number_of_choices)]  # List of student names per choice
        total_submissions = 0
        
        # Count submissions for each choice and collect student names
        for submission in submissions:
            try:
                answer = StudentAnswer.objects.get(submission=submission)
                answer_data = answer.answer_data or {}
                
                # Handle both single and multiple choice answers
                # The field is 'selected_choice_indices' as per MultipleChoiceAnswerSerializer
                selected_indices = answer_data.get('selected_choice_indices', [])
                if not isinstance(selected_indices, list):
                    selected_indices = [selected_indices] if selected_indices is not None else []
                
                student_name = submission.student.full_name
                
                for idx in selected_indices:
                    if 0 <= idx < number_of_choices:
                        choice_counts[idx] += 1
                        choice_students[idx].append(student_name)
                
                total_submissions += 1
            except StudentAnswer.DoesNotExist:
                continue
        
        # Calculate percentages
        percentages = []
        for count in choice_counts:
            if total_submissions > 0:
                percentages.append(round((count / total_submissions) * 100))
            else:
                percentages.append(0)
        
        # Build response with choice labels and student names
        choice_stats = []
        for i in range(number_of_choices):
            choice_label = choices[i].get('text', f'Choice {chr(65 + i)}') if i < len(choices) else f'Choice {chr(65 + i)}'
            is_correct = choices[i].get('is_correct', False) if i < len(choices) else False
            
            choice_stats.append({
                'index': i,
                'label': choice_label,
                'count': choice_counts[i],
                'percentage': percentages[i],
                'is_correct': is_correct,
                'students': choice_students[i]  # List of student names who selected this choice
            })
        
        # Get total number of students enrolled in the class associated with this quiz
        from students.models import StudentClassEnrollment
        enrolled_student_count = 0
        if quiz.course:
            # Get the active class for this course
            active_class = quiz.course.classes.filter(active=True).first()
            if active_class:
                enrolled_student_count = StudentClassEnrollment.objects.filter(
                    classroom=active_class
                ).count()
        
        return Response({
            'quiz_id': quiz.id,
            'quiz_title': quiz.title,
            'total_submissions': total_submissions,
            'enrolled_students': enrolled_student_count,
            'choice_stats': choice_stats
        })


class ShortAnswerSubmissionStatsView(APIView):
    """
    Get submission statistics for a short answer quiz.
    Returns all student submissions with their answers.
    """
    permission_classes = [permissions.IsAuthenticated]

    def get(self, request, quiz_id):
        try:
            quiz = Quiz.objects.get(id=quiz_id, created_by=request.user)
            print(f"[ShortAnswerSubmissionStatsView] Quiz {quiz_id} found: {quiz.title}")
        except Quiz.DoesNotExist:
            print(f"[ShortAnswerSubmissionStatsView] Quiz {quiz_id} not found for user {request.user}")
            return Response({'error': 'Quiz not found or you do not have permission to view it.'}, 
                          status=status.HTTP_404_NOT_FOUND)

        if quiz.quiz_type != QuizTypeCodes.SHORT_ANSWER:
            print(f"[ShortAnswerSubmissionStatsView] Quiz {quiz_id} is not short answer type: {quiz.quiz_type}")
            return Response({'error': 'This endpoint only supports short answer quizzes.'}, 
                          status=status.HTTP_400_BAD_REQUEST)

        # Get all submissions for this quiz
        from students.models import StudentQuizSubmission, StudentAnswer
        submissions = StudentQuizSubmission.objects.filter(quiz=quiz).select_related('student').order_by('-submitted_at')
        print(f"[ShortAnswerSubmissionStatsView] Found {submissions.count()} submissions for quiz {quiz_id}")
        
        # Get properties
        properties = quiz.properties or {}
        question_text = properties.get('question_text', '')
        
        # Collect all answers with student names
        submissions_list = []
        total_submissions = 0
        
        for submission in submissions:
            try:
                answer = StudentAnswer.objects.get(submission=submission)
                answer_data = answer.answer_data or {}
                
                submissions_list.append({
                    'id': submission.id,
                    'student_name': submission.student.full_name,
                    'answer': answer_data.get('answer_text', ''),
                    'submitted_at': submission.submitted_at.isoformat(),
                    'is_liked': getattr(submission, 'is_liked', False)  # Add is_liked if you have this field
                })
                
                total_submissions += 1
            except StudentAnswer.DoesNotExist:
                continue
        
        # Get total number of students enrolled in the class associated with this quiz
        from students.models import StudentClassEnrollment
        enrolled_student_count = 0
        if quiz.course:
            # Get the active class for this course
            active_class = quiz.course.classes.filter(active=True).first()
            if active_class:
                enrolled_student_count = StudentClassEnrollment.objects.filter(
                    classroom=active_class
                ).count()
        
        return Response({
            'quiz_id': quiz.id,
            'quiz_title': quiz.title,
            'question_text': question_text,
            'total_submissions': total_submissions,
            'enrolled_students': enrolled_student_count,
            'submissions': submissions_list
        })


# ALAA_SAJA_TODO: Add Multi-Quiz Views
# Create new view classes for managing multi-quiz
# do not forget about the permissions and the querysets :D 