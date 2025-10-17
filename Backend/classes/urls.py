from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import ClassViewSet, create_class_from_powerpoint, end_class

# Router for ViewSet endpoints
router = DefaultRouter()
router.register('', ClassViewSet, basename='class')

urlpatterns = [
    # Custom endpoints (must come before router to avoid conflicts)
    path('create-class/', create_class_from_powerpoint, name='create_class_from_ppt'),
    path('<int:class_id>/end/', end_class, name='end_class'),
    
    # ViewSet endpoints (GET /api/classes/, GET /api/classes/{id}/)
    path('', include(router.urls)),
]
