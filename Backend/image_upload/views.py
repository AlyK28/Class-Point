from rest_framework import generics, status, permissions
from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from rest_framework.parsers import MultiPartParser, FormParser
from django.shortcuts import get_object_or_404
from django.http import HttpResponse, Http404
from django.contrib.auth.models import User
from django.db.models import Q, Count, Avg
from django.utils import timezone
from django.core.files.storage import default_storage
import zipfile
import io
import os

from .models import ImageUploadSession, ImageSubmission, SessionSettings
from .serializers import (
    ImageUploadSessionSerializer, 
    ImageSubmissionSerializer, 
    ImageSubmissionListSerializer,
    SessionSettingsSerializer,
    SessionStatsSerializer,
    BulkDownloadSerializer
)


class ImageUploadSessionListCreateView(generics.ListCreateAPIView):
    """
    List all sessions for the authenticated teacher or create a new session.
    """
    serializer_class = ImageUploadSessionSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Return sessions created by the authenticated teacher."""
        return ImageUploadSession.objects.filter(teacher=self.request.user)

    def perform_create(self, serializer):
        """Create a new session with the current user as teacher."""
        serializer.save(teacher=self.request.user)


class ImageUploadSessionDetailView(generics.RetrieveUpdateDestroyAPIView):
    """
    Retrieve, update or delete a specific session.
    Only the session creator can perform these operations.
    """
    serializer_class = ImageUploadSessionSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Return sessions created by the authenticated teacher."""
        return ImageUploadSession.objects.filter(teacher=self.request.user)

    def perform_update(self, serializer):
        """Update session, ensuring only the creator can modify it."""
        session = self.get_object()
        if session.teacher != self.request.user:
            raise permissions.PermissionDenied("You can only modify your own sessions.")
        serializer.save()

    def perform_destroy(self, instance):
        """Delete session, ensuring only the creator can delete it."""
        if instance.teacher != self.request.user:
            raise permissions.PermissionDenied("You can only delete your own sessions.")
        instance.delete()


class ImageSubmissionListCreateView(generics.ListCreateAPIView):
    """
    List submissions for a specific session or create a new submission.
    Teachers can see all submissions, students can only submit.
    """
    permission_classes = [permissions.AllowAny]  # Allow anonymous submissions
    parser_classes = [MultiPartParser, FormParser]

    def get_serializer_class(self):
        if self.request.method == 'POST':
            return ImageSubmissionSerializer
        return ImageSubmissionListSerializer

    def get_queryset(self):
        """Return submissions for the specified session."""
        session_code = self.kwargs.get('session_code')
        session = get_object_or_404(ImageUploadSession, session_code=session_code)
        
        # Check if session is active
        if not session.is_active():
            return ImageSubmission.objects.none()
        
        # Return non-deleted submissions
        return ImageSubmission.objects.filter(
            session=session,
            deleted_at__isnull=True
        )

    def perform_create(self, serializer):
        """Create a new submission for the specified session."""
        session_code = self.kwargs.get('session_code')
        session = get_object_or_404(ImageUploadSession, session_code=session_code)
        
        # Check if session is active
        if not session.is_active():
            raise permissions.PermissionDenied("This session is no longer active.")
        
        # Check submission limit
        if session.submission_count >= session.max_submissions:
            raise permissions.PermissionDenied("Maximum submissions reached for this session.")
        
        # Create submission
        submission = serializer.save(session=session)
        
        # Update session submission count
        session.submission_count += 1
        session.save()


class ImageSubmissionDetailView(generics.RetrieveUpdateDestroyAPIView):
    """
    Retrieve, update or delete a specific submission.
    Only the session creator can perform these operations.
    """
    serializer_class = ImageSubmissionSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Return submissions for sessions created by the authenticated teacher."""
        return ImageSubmission.objects.filter(
            session__teacher=self.request.user,
            deleted_at__isnull=True
        )

    def perform_update(self, serializer):
        """Update submission, ensuring only the session creator can modify it."""
        submission = self.get_object()
        if submission.session.teacher != self.request.user:
            raise permissions.PermissionDenied("You can only modify submissions in your own sessions.")
        serializer.save()

    def perform_destroy(self, instance):
        """Soft delete submission."""
        if instance.session.teacher != self.request.user:
            raise permissions.PermissionDenied("You can only delete submissions in your own sessions.")
        instance.soft_delete()


@api_view(['GET'])
@permission_classes([permissions.IsAuthenticated])
def session_stats(request, session_id):
    """
    Get statistics for a specific session.
    Only the session creator can access this.
    """
    session = get_object_or_404(ImageUploadSession, id=session_id, teacher=request.user)
    
    submissions = ImageSubmission.objects.filter(session=session, deleted_at__isnull=True)
    
    # Calculate statistics
    total_submissions = submissions.count()
    total_likes = submissions.aggregate(total=Count('likes'))['total'] or 0
    liked_submissions = submissions.filter(is_liked=True).count()
    average_file_size = submissions.aggregate(avg=Avg('file_size'))['avg'] or 0
    
    # Most common format
    format_counts = submissions.values('mime_type').annotate(count=Count('id')).order_by('-count')
    most_common_format = format_counts.first()['mime_type'] if format_counts else 'N/A'
    
    # Submissions by hour (last 24 hours)
    from django.db.models.functions import TruncHour
    from django.utils import timezone
    now = timezone.now()
    yesterday = now - timezone.timedelta(hours=24)
    
    submissions_by_hour = submissions.filter(
        uploaded_at__gte=yesterday
    ).annotate(
        hour=TruncHour('uploaded_at')
    ).values('hour').annotate(count=Count('id')).order_by('hour')
    
    stats_data = {
        'total_submissions': total_submissions,
        'total_likes': total_likes,
        'liked_submissions': liked_submissions,
        'average_file_size': round(average_file_size, 2),
        'most_common_format': most_common_format,
        'submissions_by_hour': {str(item['hour']): item['count'] for item in submissions_by_hour}
    }
    
    serializer = SessionStatsSerializer(stats_data)
    return Response(serializer.data)


@api_view(['POST'])
@permission_classes([permissions.IsAuthenticated])
def toggle_like(request, submission_id):
    """
    Toggle like status for a submission.
    Only the session creator can like/unlike submissions.
    """
    submission = get_object_or_404(
        ImageSubmission, 
        id=submission_id, 
        session__teacher=request.user,
        deleted_at__isnull=True
    )
    
    submission.toggle_like()
    serializer = ImageSubmissionSerializer(submission)
    return Response(serializer.data)


@api_view(['POST'])
@permission_classes([permissions.IsAuthenticated])
def bulk_download(request, session_id):
    """
    Download multiple submissions as a ZIP file.
    Only the session creator can download submissions.
    """
    session = get_object_or_404(ImageUploadSession, id=session_id, teacher=request.user)
    
    serializer = BulkDownloadSerializer(data=request.data)
    if not serializer.is_valid():
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    submission_ids = serializer.validated_data.get('submission_ids', [])
    include_metadata = serializer.validated_data.get('include_metadata', True)
    zip_filename = serializer.validated_data.get('zip_filename', f'session_{session.session_code}_submissions.zip')
    
    # Get submissions
    if submission_ids:
        submissions = ImageSubmission.objects.filter(
            id__in=submission_ids,
            session=session,
            deleted_at__isnull=True
        )
    else:
        submissions = ImageSubmission.objects.filter(
            session=session,
            deleted_at__isnull=True
        )
    
    if not submissions.exists():
        return Response(
            {'error': 'No submissions found'}, 
            status=status.HTTP_404_NOT_FOUND
        )
    
    # Create ZIP file
    zip_buffer = io.BytesIO()
    with zipfile.ZipFile(zip_buffer, 'w', zipfile.ZIP_DEFLATED) as zip_file:
        for submission in submissions:
            if submission.image and default_storage.exists(submission.image.name):
                # Add image file
                image_path = default_storage.path(submission.image.name)
                zip_file.write(
                    image_path, 
                    f"{submission.student_name or 'anonymous'}_{submission.id}_{submission.file_name}"
                )
                
                # Add metadata if requested
                if include_metadata:
                    metadata = {
                        'submission_id': str(submission.id),
                        'student_name': submission.student_name or 'Anonymous',
                        'file_name': submission.file_name,
                        'file_size': submission.file_size,
                        'mime_type': submission.mime_type,
                        'uploaded_at': submission.uploaded_at.isoformat(),
                        'likes': submission.likes,
                        'is_liked': submission.is_liked,
                        'metadata': submission.metadata
                    }
                    
                    metadata_content = '\n'.join([f"{k}: {v}" for k, v in metadata.items()])
                    zip_file.writestr(
                        f"{submission.student_name or 'anonymous'}_{submission.id}_metadata.txt",
                        metadata_content
                    )
    
    zip_buffer.seek(0)
    
    response = HttpResponse(zip_buffer.getvalue(), content_type='application/zip')
    response['Content-Disposition'] = f'attachment; filename="{zip_filename}"'
    return response


@api_view(['GET'])
@permission_classes([permissions.AllowAny])
def public_session_view(request, session_code):
    """
    Public view for students to access a session and submit images.
    This endpoint doesn't require authentication.
    """
    session = get_object_or_404(ImageUploadSession, session_code=session_code)
    
    if not session.is_active():
        return Response(
            {'error': 'This session is no longer active'}, 
            status=status.HTTP_410_GONE
        )
    
    # Return session info for the public interface
    serializer = ImageUploadSessionSerializer(session)
    return Response(serializer.data)


@api_view(['GET'])
@permission_classes([permissions.IsAuthenticated])
def teacher_sessions(request):
    """
    Get all sessions for the authenticated teacher with basic stats.
    """
    sessions = ImageUploadSession.objects.filter(teacher=request.user).annotate(
        actual_submission_count=Count('submissions', filter=Q(submissions__deleted_at__isnull=True))
    )
    
    serializer = ImageUploadSessionSerializer(sessions, many=True)
    return Response(serializer.data)


@api_view(['POST'])
@permission_classes([permissions.IsAuthenticated])
def close_session(request, session_id):
    """
    Close an active session.
    Only the session creator can close it.
    """
    session = get_object_or_404(ImageUploadSession, id=session_id, teacher=request.user)
    
    if not session.is_active():
        return Response(
            {'error': 'Session is already closed'}, 
            status=status.HTTP_400_BAD_REQUEST
        )
    
    session.close_session()
    serializer = ImageUploadSessionSerializer(session)
    return Response(serializer.data)


class SessionSettingsView(generics.RetrieveUpdateAPIView):
    """
    Retrieve or update teacher's session settings.
    """
    serializer_class = SessionSettingsSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_object(self):
        """Get or create settings for the current user."""
        settings, created = SessionSettings.objects.get_or_create(
            teacher=self.request.user
        )
        return settings
