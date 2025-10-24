from rest_framework.routers import DefaultRouter
from django.urls import path
from .views import (
    QuizViewSet,
    CreateShortAnswerQuizView, CreateMultipleChoiceQuizView, CreateWordCloudQuizView,
    MultiQuizViewSet
)

router = DefaultRouter()
router.register('', QuizViewSet, basename='quiz')

urlpatterns = [
    path('create/short-answer/', CreateShortAnswerQuizView.as_view(), name='create_short_answer_quiz'),
    path('create/multiple-choice/', CreateMultipleChoiceQuizView.as_view(), name='create_multiple_choice_quiz'),
    path('create/word-cloud/', CreateWordCloudQuizView.as_view(), name='create_word_cloud_quiz'),
    
    # Multi-quiz URLs
    path('multi-quiz/', MultiQuizViewSet.as_view({'get': 'list', 'post': 'create'}), name='multi_quiz_list'),
    path('multi-quiz/<uuid:pk>/', MultiQuizViewSet.as_view({'get': 'retrieve', 'delete': 'destroy'}), name='multi_quiz_detail'),
]

urlpatterns += router.urls
