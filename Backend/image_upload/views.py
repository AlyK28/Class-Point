from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework.views import APIView
from django.shortcuts import get_object_or_404, render
from django.http import HttpResponse, Http404
from django.core.files.storage import default_storage
from django.conf import settings
from channels.layers import get_channel_layer
from asgiref.sync import async_to_sync
import zipfile
import io
import os
from datetime import datetime

from .models import ImageUploadSession, ImageSubmission, SessionSettings
from .serializers import (
    ImageUploadSessionSerializer, ImageSubmissionSerializer,
    ImageSubmissionListSerializer, SessionSettingsSerializer,
    SessionStatsSerializer, BulkDownloadSerializer
)


class ImageUploadSessionViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing image upload sessions.
    Teachers can create, view, and manage their sessions.
    """
    serializer_class = ImageUploadSessionSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Return sessions created by the current user."""
        return ImageUploadSession.objects.filter(teacher=self.request.user)

    def perform_create(self, serializer):
        """Set the teacher to the current user."""
        serializer.save(teacher=self.request.user)

    @action(detail=True, methods=['post'])
    def close(self, request, pk=None):
        """Close an active session."""
        session = self.get_object()
        if session.status == 'closed':
            return Response(
                {'error': 'Session is already closed.'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        session.close_session()
        
        # Send WebSocket event
        self.send_session_closed_event(session)
        
        return Response({'message': 'Session closed successfully.'})

    @action(detail=True, methods=['get'])
    def submissions(self, request, pk=None):
        """Get all submissions for a session."""
        session = self.get_object()
        submissions = session.submissions.filter(deleted_at__isnull=True)
        
        # Apply filters
        liked_only = request.query_params.get('liked', '').lower() == 'true'
        if liked_only:
            submissions = submissions.filter(is_liked=True)
        
        # Apply sorting
        sort_by = request.query_params.get('sortBy', 'timestamp')
        if sort_by == 'likes':
            submissions = submissions.order_by('-likes', '-uploaded_at')
        else:  # timestamp
            submissions = submissions.order_by('-uploaded_at')
        
        # Pagination
        limit = int(request.query_params.get('limit', 50))
        offset = int(request.query_params.get('offset', 0))
        submissions = submissions[offset:offset + limit]
        
        serializer = ImageSubmissionListSerializer(submissions, many=True)
        return Response({
            'submissions': serializer.data,
            'total': session.submissions.filter(deleted_at__isnull=True).count(),
            'hasMore': offset + limit < session.submissions.filter(deleted_at__isnull=True).count()
        })

    @action(detail=True, methods=['get'])
    def stats(self, request, pk=None):
        """Get session statistics."""
        session = self.get_object()
        submissions = session.submissions.filter(deleted_at__isnull=True)
        
        total_submissions = submissions.count()
        total_likes = sum(sub.likes for sub in submissions)
        liked_submissions = submissions.filter(is_liked=True).count()
        
        # Calculate average file size
        file_sizes = [sub.file_size for sub in submissions if sub.file_size]
        average_file_size = sum(file_sizes) / len(file_sizes) if file_sizes else 0
        
        # Most common format
        formats = [sub.mime_type.split('/')[-1] for sub in submissions]
        most_common_format = max(set(formats), key=formats.count) if formats else 'N/A'
        
        # Submissions by hour (simplified)
        submissions_by_hour = {}
        for sub in submissions:
            hour = sub.uploaded_at.hour
            submissions_by_hour[hour] = submissions_by_hour.get(hour, 0) + 1
        
        stats_data = {
            'total_submissions': total_submissions,
            'total_likes': total_likes,
            'liked_submissions': liked_submissions,
            'average_file_size': round(average_file_size, 2),
            'most_common_format': most_common_format,
            'submissions_by_hour': submissions_by_hour
        }
        
        serializer = SessionStatsSerializer(stats_data)
        return Response(serializer.data)

    @action(detail=True, methods=['post'])
    def download_all(self, request, pk=None):
        """Download all submissions as a ZIP file."""
        session = self.get_object()
        submissions = session.submissions.filter(deleted_at__isnull=True)
        
        if not submissions.exists():
            return Response(
                {'error': 'No submissions to download.'},
                status=status.HTTP_404_NOT_FOUND
            )
        
        # Create ZIP file in memory
        zip_buffer = io.BytesIO()
        with zipfile.ZipFile(zip_buffer, 'w', zipfile.ZIP_DEFLATED) as zip_file:
            for submission in submissions:
                if submission.image:
                    try:
                        # Read file from storage
                        file_path = submission.image.path
                        if os.path.exists(file_path):
                            # Create filename with student name and timestamp
                            student_name = submission.student_name or 'Anonymous'
                            timestamp = submission.uploaded_at.strftime('%Y%m%d_%H%M%S')
                            filename = f"{student_name}_{timestamp}_{submission.file_name}"
                            
                            zip_file.write(file_path, filename)
                    except Exception as e:
                        continue  # Skip files that can't be read
        
        zip_buffer.seek(0)
        
        # Return ZIP file
        response = HttpResponse(
            zip_buffer.getvalue(),
            content_type='application/zip'
        )
        response['Content-Disposition'] = f'attachment; filename="{session.name}_submissions.zip"'
        return response
    
    def send_session_closed_event(self, session):
        """Send WebSocket event when a session is closed."""
        channel_layer = get_channel_layer()
        if channel_layer:
            async_to_sync(channel_layer.group_send)(
                f'session_{session.session_code}',
                {
                    'type': 'session_closed',
                    'session_id': str(session.id),
                    'closed_at': session.closed_at.isoformat() if session.closed_at else None
                }
            )


class ImageSubmissionViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing image submissions.
    Students can upload images, teachers can manage submissions.
    """
    serializer_class = ImageSubmissionSerializer
    permission_classes = [permissions.AllowAny]  # Allow anonymous uploads

    def get_queryset(self):
        """Return submissions based on user permissions."""
        if self.request.user.is_authenticated:
            # Teachers can see all submissions in their sessions
            return ImageSubmission.objects.filter(
                session__teacher=self.request.user,
                deleted_at__isnull=True
            )
        else:
            # Anonymous users can only see submissions for specific sessions
            session_code = self.request.query_params.get('session_code')
            if session_code:
                return ImageSubmission.objects.filter(
                    session__session_code=session_code,
                    deleted_at__isnull=True
                )
            return ImageSubmission.objects.none()

    def get_permissions(self):
        """Set permissions based on action."""
        if self.action in ['create']:
            return [permissions.AllowAny()]  # Allow anonymous uploads
        elif self.action in ['update', 'partial_update', 'destroy']:
            return [permissions.IsAuthenticated()]  # Only teachers can modify
        return super().get_permissions()

    def perform_create(self, serializer):
        """Create submission and update session count."""
        session = serializer.validated_data['session']
        
        # Check if session is active
        if not session.is_active():
            raise serializers.ValidationError("This session is no longer accepting submissions.")
        
        # Check submission limit
        if session.submission_count >= session.max_submissions:
            raise serializers.ValidationError("Session has reached maximum submission limit.")
        
        # Save submission
        submission = serializer.save()
        
        # Update session submission count
        session.submission_count += 1
        session.save()
        
        # Send WebSocket event
        self.send_submission_created_event(submission)

    @action(detail=True, methods=['post'])
    def toggle_like(self, request, pk=None):
        """Toggle like status for a submission."""
        submission = self.get_object()
        
        # Check if user is the teacher who owns the session
        if request.user != submission.session.teacher:
            return Response(
                {'error': 'Only the session owner can like submissions.'},
                status=status.HTTP_403_FORBIDDEN
            )
        
        submission.toggle_like()
        
        # Send WebSocket event
        self.send_submission_liked_event(submission)
        
        serializer = self.get_serializer(submission)
        return Response(serializer.data)

    @action(detail=True, methods=['delete'])
    def soft_delete(self, request, pk=None):
        """Soft delete a submission."""
        submission = self.get_object()
        
        # Check if user is the teacher who owns the session
        if request.user != submission.session.teacher:
            return Response(
                {'error': 'Only the session owner can delete submissions.'},
                status=status.HTTP_403_FORBIDDEN
            )
        
        submission.soft_delete()
        
        # Send WebSocket event
        self.send_submission_deleted_event(submission)
        
        return Response({'message': 'Submission deleted successfully.'})

    @action(detail=True, methods=['get'])
    def download(self, request, pk=None):
        """Download the original image file."""
        submission = self.get_object()
        
        if not submission.image:
            raise Http404("Image file not found.")
        
        try:
            # Read file from storage
            file_path = submission.image.path
            if os.path.exists(file_path):
                with open(file_path, 'rb') as f:
                    response = HttpResponse(f.read(), content_type=submission.mime_type)
                    response['Content-Disposition'] = f'attachment; filename="{submission.file_name}"'
                    return response
            else:
                raise Http404("Image file not found on disk.")
        except Exception as e:
            raise Http404("Error reading image file.")
    
    def send_submission_created_event(self, submission):
        """Send WebSocket event when a new submission is created."""
        channel_layer = get_channel_layer()
        if channel_layer:
            async_to_sync(channel_layer.group_send)(
                f'session_{submission.session.session_code}',
                {
                    'type': 'submission_created',
                    'submission': {
                        'id': str(submission.id),
                        'session_id': str(submission.session.id),
                        'student_name': submission.student_name,
                        'image_url': submission.get_image_url(),
                        'thumbnail_url': submission.get_thumbnail_url(),
                        'uploaded_at': submission.uploaded_at.isoformat()
                    }
                }
            )
    
    def send_submission_liked_event(self, submission):
        """Send WebSocket event when a submission is liked/unliked."""
        channel_layer = get_channel_layer()
        if channel_layer:
            async_to_sync(channel_layer.group_send)(
                f'session_{submission.session.session_code}',
                {
                    'type': 'submission_liked',
                    'submission_id': str(submission.id),
                    'likes': submission.likes,
                    'is_liked': submission.is_liked
                }
            )
    
    def send_submission_deleted_event(self, submission):
        """Send WebSocket event when a submission is deleted."""
        channel_layer = get_channel_layer()
        if channel_layer:
            async_to_sync(channel_layer.group_send)(
                f'session_{submission.session.session_code}',
                {
                    'type': 'submission_deleted',
                    'submission_id': str(submission.id)
                }
            )


class PublicSessionView(APIView):
    """
    Public view for students to access session information and upload images.
    No authentication required.
    """
    permission_classes = [permissions.AllowAny]

    def get(self, request, session_code):
        """Get session information for students."""
        try:
            session = ImageUploadSession.objects.get(
                session_code=session_code,
                status='active'
            )
        except ImageUploadSession.DoesNotExist:
            return Response(
                {'error': 'Session not found or inactive.'},
                status=status.HTTP_404_NOT_FOUND
            )
        
        # Return session info for students
        return Response({
            'session_code': session.session_code,
            'name': session.name,
            'question': session.question,
            'submission_count': session.submission_count,
            'max_submissions': session.max_submissions,
            'allow_anonymous': session.allow_anonymous
        })

    def post(self, request, session_code):
        """Upload an image to the session."""
        try:
            session = ImageUploadSession.objects.get(
                session_code=session_code,
                status='active'
            )
        except ImageUploadSession.DoesNotExist:
            return Response(
                {'error': 'Session not found or inactive.'},
                status=status.HTTP_404_NOT_FOUND
            )
        
        # Check submission limit
        if session.submission_count >= session.max_submissions:
            return Response(
                {'error': 'Session has reached maximum submission limit.'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Create submission
        serializer = ImageSubmissionSerializer(data=request.data)
        if serializer.is_valid():
            # Add session to validated data
            serializer.validated_data['session'] = session
            submission = serializer.save()
            
            # Update session count
            session.submission_count += 1
            session.save()
            
            return Response(
                {
                    'success': True,
                    'submission_id': submission.id,
                    'image_url': submission.get_image_url(),
                    'thumbnail_url': submission.get_thumbnail_url(),
                    'uploaded_at': submission.uploaded_at
                },
                status=status.HTTP_201_CREATED
            )
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class SessionSettingsViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing teacher session settings.
    """
    serializer_class = SessionSettingsSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        """Return settings for the current user."""
        return SessionSettings.objects.filter(teacher=self.request.user)

    def perform_create(self, serializer):
        """Set the teacher to the current user."""
        serializer.save(teacher=self.request.user)

    def get_object(self):
        """Get or create settings for the current user."""
        settings_obj, created = SessionSettings.objects.get_or_create(
            teacher=self.request.user
        )
        return settings_obj


class StudentUploadView(APIView):
    """
    View to serve the student upload interface.
    """
    permission_classes = [permissions.AllowAny]

    def get(self, request, session_code):
        """Serve the student upload page."""
        try:
            session = ImageUploadSession.objects.get(
                session_code=session_code,
                status='active'
            )
        except ImageUploadSession.DoesNotExist:
            return render(request, 'image_upload/error.html', {
                'error': 'Session not found or inactive.',
                'session_code': session_code
            })
        
        return render(request, 'image_upload/student_upload.html', {
            'session': session
        })
