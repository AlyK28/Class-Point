from rest_framework.routers import DefaultRouter
from .views import (
    QuizTypeViewSet, QuizViewSet,
    MultipleChoiceOptionsViewSet, WordCloudOptionsViewSet,
    ShortAnswerOptionsViewSet, DrawingOptionsViewSet, ImageUploadOptionsViewSet
)

router = DefaultRouter()
router.register('types', QuizTypeViewSet, basename='quiztype')
router.register('', QuizViewSet, basename='quiz')
router.register('mcq-options', MultipleChoiceOptionsViewSet, basename='mcqoptions')
router.register('wordcloud-options', WordCloudOptionsViewSet, basename='wordcloudoptions')
router.register('shortanswer-options', ShortAnswerOptionsViewSet, basename='shortansweroptions')
router.register('drawing-options', DrawingOptionsViewSet, basename='drawingoptions')
router.register('imageupload-options', ImageUploadOptionsViewSet, basename='imageuploadoptions')

urlpatterns = router.urls
