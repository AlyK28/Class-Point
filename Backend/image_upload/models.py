from django.db import models
from django.contrib.auth.models import User
from django.core.exceptions import ValidationError
from django.utils import timezone
import uuid
import random
import string
import os
from PIL import Image
from io import BytesIO
from django.core.files.base import ContentFile


def generate_thumbnail(image_field, size=(200, 200)):
    """Generate thumbnail for uploaded image."""
    try:
        img = Image.open(image_field)
        img.thumbnail(size, Image.Resampling.LANCZOS)
        
        # Convert to RGB if necessary (for PNG with transparency)
        if img.mode in ('RGBA', 'LA', 'P'):
            background = Image.new('RGB', img.size, (255, 255, 255))
            if img.mode == 'P':
                img = img.convert('RGBA')
            background.paste(img, mask=img.split()[-1] if img.mode == 'RGBA' else None)
            img = background
        
        thumb_io = BytesIO()
        img.save(thumb_io, format='JPEG', quality=85)
        thumb_io.seek(0)
        
        return ContentFile(thumb_io.getvalue())
    except Exception as e:
        return None


class ImageUploadSession(models.Model):
    """
    Represents a live image upload session created by a teacher.
    Based on PRD specifications for session management.
    """
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    session_code = models.CharField(max_length=6, unique=True, editable=False)
    name = models.CharField(max_length=255)
    question = models.TextField(blank=True, null=True, help_text="Optional prompt for students")
    teacher = models.ForeignKey(User, on_delete=models.CASCADE, related_name='image_sessions')
    status = models.CharField(
        max_length=20,
        choices=[
            ('active', 'Active'),
            ('closed', 'Closed'),
        ],
        default='active'
    )
    allow_anonymous = models.BooleanField(default=True)
    max_submissions = models.PositiveIntegerField(default=100, help_text="Maximum number of submissions allowed")
    submission_count = models.PositiveIntegerField(default=0)
    created_at = models.DateTimeField(auto_now_add=True)
    closed_at = models.DateTimeField(blank=True, null=True)

    class Meta:
        ordering = ['-created_at']

    def clean(self):
        """Validate session constraints."""
        super().clean()
        if self.submission_count > self.max_submissions:
            raise ValidationError("Submission count cannot exceed maximum submissions.")

    def save(self, *args, **kwargs):
        if not self.session_code:
            self.session_code = self.generate_unique_code()
        super().save(*args, **kwargs)

    def generate_unique_code(self):
        """Generate a unique 6-character session code."""
        while True:
            code = ''.join(random.choices(string.ascii_uppercase + string.digits, k=6))
            if not ImageUploadSession.objects.filter(session_code=code).exists():
                return code

    def close_session(self):
        """Close the session and update timestamp."""
        self.status = 'closed'
        self.closed_at = timezone.now()
        self.save()

    def is_active(self):
        """Check if session is currently active."""
        return self.status == 'active'

    def get_public_url(self):
        """Get the public URL for students to access this session."""
        # This would be configured based on your domain
        return f"/upload/{self.session_code}/"

    def __str__(self):
        return f"{self.name} ({self.session_code}) - {self.status}"


class ImageSubmission(models.Model):
    """
    Represents an image uploaded by a student to a session.
    Based on PRD specifications for submission management.
    """
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    session = models.ForeignKey(ImageUploadSession, on_delete=models.CASCADE, related_name='submissions')
    student_name = models.CharField(max_length=255, blank=True, null=True)
    image = models.ImageField(upload_to='image_uploads/%Y/%m/%d/')
    thumbnail = models.ImageField(upload_to='image_thumbnails/%Y/%m/%d/', blank=True, null=True)
    file_name = models.CharField(max_length=255)
    file_size = models.PositiveIntegerField(help_text="File size in bytes")
    mime_type = models.CharField(max_length=100)
    likes = models.PositiveIntegerField(default=0)
    is_liked = models.BooleanField(default=False, help_text="Teacher liked this submission")
    metadata = models.JSONField(default=dict, blank=True, help_text="Image metadata (dimensions, EXIF, etc.)")
    uploaded_at = models.DateTimeField(auto_now_add=True)
    deleted_at = models.DateTimeField(blank=True, null=True, help_text="Soft delete timestamp")

    class Meta:
        ordering = ['-uploaded_at']

    def save(self, *args, **kwargs):
        # Generate thumbnail if image is being saved for the first time
        if self.pk is None and self.image:
            thumbnail = generate_thumbnail(self.image)
            if thumbnail:
                self.thumbnail.save(
                    f"thumb_{self.image.name}",
                    thumbnail,
                    save=False
                )
        super().save(*args, **kwargs)

    def soft_delete(self):
        """Soft delete the submission."""
        self.deleted_at = timezone.now()
        self.save()

    def is_deleted(self):
        """Check if submission is soft deleted."""
        return self.deleted_at is not None

    def toggle_like(self):
        """Toggle the like status and update count."""
        self.is_liked = not self.is_liked
        if self.is_liked:
            self.likes += 1
        else:
            self.likes = max(0, self.likes - 1)
        self.save()

    def get_image_url(self):
        """Get the URL for the full-size image."""
        if self.image:
            return self.image.url
        return None

    def get_thumbnail_url(self):
        """Get the URL for the thumbnail image."""
        if self.thumbnail:
            return self.thumbnail.url
        return self.get_image_url()  # Fallback to full image

    def __str__(self):
        student = self.student_name or "Anonymous"
        return f"{student} - {self.session.name} ({self.uploaded_at.strftime('%Y-%m-%d %H:%M')})"


class SessionSettings(models.Model):
    """
    Teacher settings for image upload sessions.
    Based on PRD Phase 2 specifications.
    """
    teacher = models.OneToOneField(User, on_delete=models.CASCADE, related_name='image_upload_settings')
    auto_download = models.BooleanField(default=False)
    max_file_size_mb = models.PositiveIntegerField(default=10)
    allowed_formats = models.JSONField(default=list, help_text="List of allowed file formats")
    default_anonymous = models.BooleanField(default=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def __str__(self):
        return f"Settings for {self.teacher.username}"

    def get_allowed_formats_list(self):
        """Get allowed formats as a list."""
        if not self.allowed_formats:
            return ['jpg', 'jpeg', 'png', 'gif', 'webp']
        return self.allowed_formats

    def get_max_file_size_bytes(self):
        """Get max file size in bytes."""
        return self.max_file_size_mb * 1024 * 1024
