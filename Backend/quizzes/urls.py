from rest_framework.routers import DefaultRouter
from django.urls import path
from .views import (
    QuizViewSet,
    CreateShortAnswerQuizView, CreateMultipleChoiceQuizView, CreateWordCloudQuizView
)

router = DefaultRouter()
router.register('', QuizViewSet, basename='quiz')

urlpatterns = [
    path('create/short-answer/', CreateShortAnswerQuizView.as_view(), name='create_short_answer_quiz'),
    path('create/multiple-choice/', CreateMultipleChoiceQuizView.as_view(), name='create_multiple_choice_quiz'),
    path('create/word-cloud/', CreateWordCloudQuizView.as_view(), name='create_word_cloud_quiz'),
]

urlpatterns += router.urls
