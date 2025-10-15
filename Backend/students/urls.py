from rest_framework.routers import DefaultRouter
from django.urls import path
from .views import (
    StudentViewSet,
    StudentClassEnrollmentViewSet,
    StudentQuizSubmissionViewSet,
    StudentAnswerViewSet,
    JoinClassView
)

router = DefaultRouter()
router.register('students', StudentViewSet, basename='student')
router.register('enrollments', StudentClassEnrollmentViewSet, basename='enrollment')
router.register('submissions', StudentQuizSubmissionViewSet, basename='submission')
router.register('answers', StudentAnswerViewSet, basename='answer')

urlpatterns = [
    path('join/', JoinClassView.as_view(), name='join-class'),
]

urlpatterns += router.urls
