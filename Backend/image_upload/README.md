# Image Upload Feature for Class-Point

This is a standalone image upload feature that can be integrated with the main Class-Point application. It implements the specifications from the PowerPoint Real-Time Image Submission Add-in PRD.

## Features

### ✅ Implemented (MVP)
- **Session Management**: Create image upload sessions with unique codes
- **Student Upload Interface**: Mobile-responsive web interface for image uploads
- **Teacher Dashboard**: Real-time grid view of submissions with like/delete functionality
- **PowerPoint Integration**: Task pane add-in for teachers
- **Real-time Updates**: WebSocket support for live submission updates
- **File Processing**: Automatic thumbnail generation and image optimization
- **QR Code Generation**: Automatic QR codes for easy student access

### 🔄 Ready for Integration
- **API Endpoints**: Complete REST API following PRD specifications
- **Database Models**: Django models for sessions and submissions
- **Authentication**: Compatible with existing Class-Point auth system
- **File Storage**: Configurable local/S3 storage with CDN support

## Quick Start

### 1. Integration with Main App

Add to your main Django project:

```python
# settings.py
INSTALLED_APPS = [
    # ... existing apps
    'image_upload',
]

# Add image upload settings
from image_upload.settings import IMAGE_UPLOAD_SETTINGS
```

```python
# main urls.py
urlpatterns = [
    # ... existing patterns
    path('image_upload/', include('image_upload.urls')),
]
```

### 2. Database Migration

```bash
python manage.py makemigrations image_upload
python manage.py migrate
```

### 3. Install Dependencies

```bash
pip install -r image_upload/requirements.txt
```

### 4. Configure Settings

Copy settings from `image_upload/settings.py` to your main settings file.

## API Endpoints

### Teacher Endpoints (Authenticated)
- `POST /image_upload/api/sessions/` - Create new session
- `GET /image_upload/api/sessions/` - List teacher's sessions
- `GET /image_upload/api/sessions/{id}/submissions/` - Get session submissions
- `POST /image_upload/api/sessions/{id}/close/` - Close session
- `POST /image_upload/api/sessions/{id}/download-all/` - Download all submissions

### Student Endpoints (Public)
- `GET /image_upload/upload/{session_code}/` - Student upload interface
- `POST /image_upload/api/upload/{session_code}/` - Upload image

### Submission Management
- `POST /image_upload/api/submissions/{id}/like/` - Toggle like
- `DELETE /image_upload/api/submissions/{id}/delete/` - Delete submission
- `GET /image_upload/api/submissions/{id}/download/` - Download image

## WebSocket Events

Connect to: `ws://localhost:8000/ws/session/{session_code}/`

Events:
- `submission_created` - New image uploaded
- `submission_liked` - Submission liked/unliked
- `submission_deleted` - Submission deleted
- `session_closed` - Session closed

## PowerPoint Add-in

### Installation
1. Copy `image_upload/static/image_upload/manifest.xml` to your web server
2. Update URLs in manifest to point to your server
3. Sideload the add-in in PowerPoint

### Features
- Create sessions directly from PowerPoint
- Real-time submission monitoring
- QR code display for students
- Like/delete submissions
- Download all submissions

## Student Interface

### Access
Students access via: `https://yourdomain.com/image_upload/upload/{session_code}/`

### Features
- Mobile-responsive design
- Camera capture and gallery upload
- Image preview and basic editing
- Real-time submission count
- Anonymous or named submissions

## Configuration

### File Storage
```python
# Local storage (default)
IMAGE_UPLOAD_SETTINGS = {
    'USE_S3': False,
}

# S3 storage (production)
IMAGE_UPLOAD_SETTINGS = {
    'USE_S3': True,
    'S3_BUCKET_NAME': 'your-bucket',
    'S3_REGION': 'us-east-1',
}
```

### WebSocket
```python
# Redis backend (recommended)
CHANNEL_LAYERS = {
    'default': {
        'BACKEND': 'channels_redis.core.RedisChannelLayer',
        'CONFIG': {
            "hosts": [('127.0.0.1', 6379)],
        },
    },
}
```

## Security Features

- File type validation (MIME + magic bytes)
- File size limits (10MB default)
- Image re-encoding to strip EXIF data
- Rate limiting on uploads
- Input sanitization
- CORS protection

## Performance

- Automatic thumbnail generation
- Progressive image loading
- WebSocket for real-time updates
- Efficient database queries
- CDN support for static files

## Testing

```bash
# Run tests
python manage.py test image_upload

# Test WebSocket
python manage.py test image_upload.tests.test_websocket

# Test file uploads
python manage.py test image_upload.tests.test_uploads
```

## Deployment

### Production Checklist
- [ ] Configure S3 storage
- [ ] Set up Redis for WebSocket
- [ ] Configure CDN
- [ ] Set up SSL certificates
- [ ] Configure CORS properly
- [ ] Set up monitoring
- [ ] Configure backup strategy

### Environment Variables
```bash
# Required
SECRET_KEY=your-secret-key
DEBUG=False

# Optional (for S3)
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_STORAGE_BUCKET_NAME=your-bucket

# Optional (for Redis)
REDIS_URL=redis://localhost:6379/0
```

## Integration with Main Class-Point

This feature is designed to integrate seamlessly with your existing Class-Point application:

1. **Shared Authentication**: Uses existing user system
2. **Compatible Models**: Can be linked to existing Course/Class models
3. **Consistent API**: Follows same patterns as existing endpoints
4. **Shared Settings**: Integrates with existing configuration

### Linking to Existing Classes
```python
# In your existing Class model, add:
class Class(models.Model):
    # ... existing fields
    image_upload_session = models.OneToOneField(
        'image_upload.ImageUploadSession',
        on_delete=models.SET_NULL,
        null=True,
        blank=True
    )
```

## Support

For issues or questions:
1. Check the PRD document for specifications
2. Review the API documentation
3. Test with the provided examples
4. Check Django logs for errors

## Future Enhancements

Based on PRD Phase 2 and 3:
- Advanced image editing tools
- AI content moderation
- Analytics and reporting
- Multi-teacher organizations
- LMS integration
- Mobile apps
