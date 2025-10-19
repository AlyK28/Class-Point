from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import (
    ImageUploadSessionViewSet,
    ImageSubmissionViewSet,
    PublicSessionView,
    SessionSettingsViewSet,
    StudentUploadView
)

# Create router for ViewSets
router = DefaultRouter()
router.register('sessions', ImageUploadSessionViewSet, basename='session')
router.register('submissions', ImageSubmissionViewSet, basename='submission')
router.register('settings', SessionSettingsViewSet, basename='settings')

urlpatterns = [
    # API routes for authenticated users (teachers)
    path('api/', include(router.urls)),
    
    # Public routes for students (no authentication required)
    path('upload/<str:session_code>/', StudentUploadView.as_view(), name='student-upload'),
    path('api/upload/<str:session_code>/', PublicSessionView.as_view(), name='public-session'),
    
    # Legacy API routes for compatibility
    path('api/sessions/', ImageUploadSessionViewSet.as_view({'get': 'list', 'post': 'create'}), name='session-list'),
    path('api/sessions/<uuid:pk>/', ImageUploadSessionViewSet.as_view({'get': 'retrieve', 'put': 'update', 'patch': 'partial_update', 'delete': 'destroy'}), name='session-detail'),
    path('api/sessions/<uuid:pk>/submissions/', ImageUploadSessionViewSet.as_view({'get': 'submissions'}), name='session-submissions'),
    path('api/sessions/<uuid:pk>/close/', ImageUploadSessionViewSet.as_view({'post': 'close'}), name='session-close'),
    path('api/sessions/<uuid:pk>/stats/', ImageUploadSessionViewSet.as_view({'get': 'stats'}), name='session-stats'),
    path('api/sessions/<uuid:pk>/download-all/', ImageUploadSessionViewSet.as_view({'post': 'download_all'}), name='session-download-all'),
    
    path('api/submissions/', ImageSubmissionViewSet.as_view({'get': 'list', 'post': 'create'}), name='submission-list'),
    path('api/submissions/<uuid:pk>/', ImageSubmissionViewSet.as_view({'get': 'retrieve', 'put': 'update', 'patch': 'partial_update', 'delete': 'destroy'}), name='submission-detail'),
    path('api/submissions/<uuid:pk>/like/', ImageSubmissionViewSet.as_view({'post': 'toggle_like'}), name='submission-like'),
    path('api/submissions/<uuid:pk>/delete/', ImageSubmissionViewSet.as_view({'delete': 'soft_delete'}), name='submission-delete'),
    path('api/submissions/<uuid:pk>/download/', ImageSubmissionViewSet.as_view({'get': 'download'}), name='submission-download'),
]
