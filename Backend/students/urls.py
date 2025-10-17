from rest_framework.routers import DefaultRouter
from django.urls import path
from .views import (
    StudentViewSet,
    StudentClassEnrollmentViewSet,
    StudentQuizSubmissionViewSet,
    StudentAnswerViewSet,
    JoinClassView,
    StudentQuizListView
)

router = DefaultRouter()
router.register('students', StudentViewSet, basename='student')
router.register('enrollments', StudentClassEnrollmentViewSet, basename='enrollment')
router.register('submissions', StudentQuizSubmissionViewSet, basename='submission')
router.register('answers', StudentAnswerViewSet, basename='answer')

urlpatterns = [
    path('join/', JoinClassView.as_view(), name='join-class'),
    path('quizzes/', StudentQuizListView.as_view(), name='student-quiz-list'),
    # Direct list/retrieve for students at /api/students/
    path('', StudentViewSet.as_view({'get': 'list'}), name='student-list-root'),
    path('<int:pk>/', StudentViewSet.as_view({'get': 'retrieve'}), name='student-detail-root'),
]

urlpatterns += router.urls
