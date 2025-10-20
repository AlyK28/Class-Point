from rest_framework import serializers
from django.contrib.auth.models import User
from .models import ImageUploadSession, ImageSubmission, SessionSettings
import qrcode
import io
import base64


class ImageUploadSessionSerializer(serializers.ModelSerializer):
    """Serializer for ImageUploadSession model."""
    teacher_username = serializers.CharField(source='teacher.username', read_only=True)
    qr_code = serializers.SerializerMethodField()
    public_url = serializers.SerializerMethodField()
    is_active = serializers.SerializerMethodField()
    
    class Meta:
        model = ImageUploadSession
        fields = [
            'id', 'session_code', 'name', 'question', 'teacher', 'teacher_username',
            'status', 'allow_anonymous', 'max_submissions', 'submission_count',
            'created_at', 'closed_at', 'qr_code', 'public_url', 'is_active'
        ]
        read_only_fields = ['id', 'session_code', 'teacher', 'submission_count', 'created_at', 'closed_at']

    def get_qr_code(self, obj):
        """Generate QR code for the session."""
        try:
            # Generate QR code
            qr = qrcode.QRCode(version=1, box_size=10, border=5)
            qr.add_data(obj.get_public_url())
            qr.make(fit=True)
            
            # Create image
            img = qr.make_image(fill_color="black", back_color="white")
            
            # Convert to base64
            buffer = io.BytesIO()
            img.save(buffer, format='PNG')
            buffer.seek(0)
            img_str = base64.b64encode(buffer.getvalue()).decode()
            
            return f"data:image/png;base64,{img_str}"
        except Exception:
            return None

    def get_public_url(self, obj):
        """Get the public URL for students to access this session."""
        return obj.get_public_url()

    def get_is_active(self, obj):
        """Check if session is currently active."""
        return obj.is_active()

    def create(self, validated_data):
        """Create a new session with the current user as teacher."""
        request = self.context.get('request')
        if request and hasattr(request, 'user'):
            validated_data['teacher'] = request.user
        return super().create(validated_data)


class ImageSubmissionSerializer(serializers.ModelSerializer):
    """Serializer for ImageSubmission model."""
    image_url = serializers.SerializerMethodField()
    thumbnail_url = serializers.SerializerMethodField()
    session_name = serializers.CharField(source='session.name', read_only=True)
    session_code = serializers.CharField(source='session.session_code', read_only=True)
    
    class Meta:
        model = ImageSubmission
        fields = [
            'id', 'session', 'session_name', 'session_code', 'student_name',
            'image', 'image_url', 'thumbnail', 'thumbnail_url', 'file_name',
            'file_size', 'mime_type', 'likes', 'is_liked', 'metadata',
            'uploaded_at', 'deleted_at'
        ]
        read_only_fields = [
            'id', 'file_name', 'file_size', 'mime_type', 'likes', 'is_liked',
            'metadata', 'uploaded_at', 'deleted_at', 'thumbnail'
        ]

    def get_image_url(self, obj):
        """Get the URL for the full-size image."""
        return obj.get_image_url()

    def get_thumbnail_url(self, obj):
        """Get the URL for the thumbnail image."""
        return obj.get_thumbnail_url()

    def validate_image(self, value):
        """Validate uploaded image."""
        # Check file size (10MB limit)
        max_size = 10 * 1024 * 1024  # 10MB
        if value.size > max_size:
            raise serializers.ValidationError("File size cannot exceed 10MB.")
        
        # Check file type
        allowed_types = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
        if value.content_type not in allowed_types:
            raise serializers.ValidationError("Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed.")
        
        return value

    def create(self, validated_data):
        """Create a new submission with metadata."""
        # Extract file information
        image = validated_data.get('image')
        if image:
            validated_data['file_name'] = image.name
            validated_data['file_size'] = image.size
            validated_data['mime_type'] = image.content_type
            
            # Extract basic metadata
            try:
                from PIL import Image as PILImage
                with PILImage.open(image) as img:
                    validated_data['metadata'] = {
                        'width': img.width,
                        'height': img.height,
                        'format': img.format,
                        'mode': img.mode
                    }
            except Exception:
                validated_data['metadata'] = {}
        
        return super().create(validated_data)


class ImageSubmissionListSerializer(serializers.ModelSerializer):
    """Lightweight serializer for listing submissions."""
    image_url = serializers.SerializerMethodField()
    thumbnail_url = serializers.SerializerMethodField()
    
    class Meta:
        model = ImageSubmission
        fields = [
            'id', 'student_name', 'image_url', 'thumbnail_url',
            'likes', 'is_liked', 'uploaded_at'
        ]

    def get_image_url(self, obj):
        return obj.get_image_url()

    def get_thumbnail_url(self, obj):
        return obj.get_thumbnail_url()


class SessionSettingsSerializer(serializers.ModelSerializer):
    """Serializer for teacher session settings."""
    
    class Meta:
        model = SessionSettings
        fields = [
            'auto_download', 'max_file_size_mb', 'allowed_formats',
            'default_anonymous', 'created_at', 'updated_at'
        ]
        read_only_fields = ['created_at', 'updated_at']

    def validate_allowed_formats(self, value):
        """Validate allowed formats list."""
        valid_formats = ['jpg', 'jpeg', 'png', 'gif', 'webp']
        if not isinstance(value, list):
            raise serializers.ValidationError("Allowed formats must be a list.")
        
        for format_name in value:
            if format_name.lower() not in valid_formats:
                raise serializers.ValidationError(f"Invalid format: {format_name}")
        
        return [f.lower() for f in value]

    def validate_max_file_size_mb(self, value):
        """Validate max file size."""
        if value < 1 or value > 50:
            raise serializers.ValidationError("Max file size must be between 1 and 50 MB.")
        return value


class SessionStatsSerializer(serializers.Serializer):
    """Serializer for session statistics."""
    total_submissions = serializers.IntegerField()
    total_likes = serializers.IntegerField()
    liked_submissions = serializers.IntegerField()
    average_file_size = serializers.FloatField()
    most_common_format = serializers.CharField()
    submissions_by_hour = serializers.DictField()


class BulkDownloadSerializer(serializers.Serializer):
    """Serializer for bulk download requests."""
    submission_ids = serializers.ListField(
        child=serializers.UUIDField(),
        required=False,
        help_text="Specific submission IDs to download. If empty, downloads all."
    )
    include_metadata = serializers.BooleanField(default=True)
    zip_filename = serializers.CharField(max_length=255, required=False)

