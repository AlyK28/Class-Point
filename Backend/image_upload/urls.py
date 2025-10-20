from django.urls import path, include
from rest_framework.routers import DefaultRouter
from . import views

# Create a router for any ViewSets (if we add them later)
router = DefaultRouter()

urlpatterns = [
    # Session management endpoints
    path('sessions/', views.ImageUploadSessionListCreateView.as_view(), name='session-list-create'),
    path('sessions/<uuid:pk>/', views.ImageUploadSessionDetailView.as_view(), name='session-detail'),
    path('sessions/<uuid:session_id>/stats/', views.session_stats, name='session-stats'),
    path('sessions/<uuid:session_id>/close/', views.close_session, name='close-session'),
    path('sessions/<uuid:session_id>/download/', views.bulk_download, name='bulk-download'),
    path('teacher/sessions/', views.teacher_sessions, name='teacher-sessions'),
    
    # Submission endpoints
    path('sessions/<str:session_code>/submissions/', views.ImageSubmissionListCreateView.as_view(), name='submission-list-create'),
    path('submissions/<uuid:pk>/', views.ImageSubmissionDetailView.as_view(), name='submission-detail'),
    path('submissions/<uuid:submission_id>/like/', views.toggle_like, name='toggle-like'),
    
    # Public endpoints (no authentication required)
    path('public/session/<str:session_code>/', views.public_session_view, name='public-session'),
    
    # Settings endpoints
    path('settings/', views.SessionSettingsView.as_view(), name='session-settings'),
    
    # Include router URLs
    path('', include(router.urls)),
]
