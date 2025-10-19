# Image Upload Feature Integration Guide

This guide explains how to integrate the standalone image upload feature with your existing Class-Point application.

## 🎯 What's Been Created

I've developed a complete image upload feature that implements the PRD specifications:

### ✅ Core Features Implemented
- **Session Management**: Create/manage image upload sessions
- **Student Interface**: Mobile-responsive upload interface
- **Teacher Dashboard**: Real-time submission monitoring
- **PowerPoint Add-in**: Task pane integration
- **Real-time Updates**: WebSocket support
- **File Processing**: Thumbnail generation and optimization
- **API Endpoints**: Complete REST API

### 📁 File Structure Created
```
image_upload/
├── models.py              # Database models
├── serializers.py         # API serializers
├── views.py              # REST API views
├── urls.py               # URL routing
├── admin.py              # Django admin
├── consumers.py          # WebSocket consumers
├── routing.py            # WebSocket routing
├── settings.py           # Configuration
├── requirements.txt      # Dependencies
├── README.md            # Documentation
├── templates/
│   └── image_upload/
│       ├── student_upload.html
│       └── error.html
└── static/
    └── image_upload/
        ├── powerpoint_addin.html
        ├── manifest.xml
        └── commands.html
```

## 🚀 Integration Steps

### Step 1: Add to Django Settings

Add to your main `settings.py`:

```python
INSTALLED_APPS = [
    # ... existing apps
    'image_upload',
    'channels',  # For WebSocket support
    'corsheaders',  # For CORS support
]

# Add image upload settings
from image_upload.settings import IMAGE_UPLOAD_SETTINGS
```

### Step 2: Update Main URLs

Add to your main `urls.py`:

```python
from django.urls import path, include

urlpatterns = [
    # ... existing patterns
    path('image_upload/', include('image_upload.urls')),
]
```

### Step 3: Install Dependencies

```bash
pip install -r image_upload/requirements.txt
```

### Step 4: Database Migration

```bash
python manage.py makemigrations image_upload
python manage.py migrate
```

### Step 5: Configure WebSocket (Optional)

If you want real-time updates, add to `settings.py`:

```python
ASGI_APPLICATION = 'classpoint_backend.asgi.application'

CHANNEL_LAYERS = {
    'default': {
        'BACKEND': 'channels_redis.core.RedisChannelLayer',
        'CONFIG': {
            "hosts": [('127.0.0.1', 6379)],
        },
    },
}
```

## 🔗 Linking with Existing Models

### Option 1: Simple Integration
The feature works independently - teachers can create sessions without linking to existing classes.

### Option 2: Link to Existing Classes
Add to your existing `Class` model:

```python
# In classes/models.py
class Class(models.Model):
    # ... existing fields
    image_upload_session = models.OneToOneField(
        'image_upload.ImageUploadSession',
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='linked_class'
    )
```

## 🎮 How to Use

### For Teachers (PowerPoint Add-in)
1. Open PowerPoint
2. Load the add-in (sideload manifest.xml)
3. Click "Interactive Images" in the ribbon
4. Create a new session
5. Share QR code or link with students
6. Monitor submissions in real-time

### For Students (Web Interface)
1. Scan QR code or visit shared link
2. Upload image from camera or gallery
3. Optionally enter name
4. Submit image

### API Usage
```python
# Create session
POST /image_upload/api/sessions/
{
    "name": "Plant Cell Diagrams",
    "question": "Take a photo of your cell diagram"
}

# Upload image (student)
POST /image_upload/api/upload/{session_code}/
{
    "image": <file>,
    "student_name": "John Doe"
}
```

## 🔧 Configuration Options

### File Storage
```python
# Local storage (default)
IMAGE_UPLOAD_SETTINGS = {
    'USE_S3': False,
    'MAX_FILE_SIZE_MB': 10,
}

# S3 storage (production)
IMAGE_UPLOAD_SETTINGS = {
    'USE_S3': True,
    'S3_BUCKET_NAME': 'your-bucket',
    'S3_REGION': 'us-east-1',
}
```

### Security
```python
# File validation
ALLOWED_IMAGE_FORMATS = ['jpg', 'jpeg', 'png', 'gif', 'webp']
MAX_FILE_SIZE_MB = 10

# CORS settings
CORS_ALLOWED_ORIGINS = [
    "https://yourdomain.com",
]
```

## 🧪 Testing

### Test the API
```bash
# Create a session
curl -X POST http://localhost:8000/image_upload/api/sessions/ \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Session", "question": "Upload a test image"}'

# Upload an image
curl -X POST http://localhost:8000/image_upload/api/upload/SESSION_CODE/ \
  -F "image=@test.jpg" \
  -F "student_name=Test Student"
```

### Test the Student Interface
1. Create a session via API
2. Visit: `http://localhost:8000/image_upload/upload/SESSION_CODE/`
3. Upload an image
4. Check the teacher dashboard

## 🚀 Deployment

### Production Checklist
- [ ] Configure S3 storage
- [ ] Set up Redis for WebSocket
- [ ] Configure CDN
- [ ] Set up SSL certificates
- [ ] Update PowerPoint add-in URLs
- [ ] Configure CORS properly
- [ ] Set up monitoring

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

## 🔄 Future Integration Ideas

### Phase 2 Enhancements
1. **Link to Quiz System**: Connect image uploads to existing quiz types
2. **Teacher Dashboard**: Integrate with existing teacher interface
3. **Analytics**: Add to existing reporting system
4. **Authentication**: Use existing JWT system

### Phase 3 Features
1. **AI Integration**: Add content moderation
2. **LMS Integration**: Connect to existing course management
3. **Mobile Apps**: Extend existing mobile functionality

## 🆘 Troubleshooting

### Common Issues
1. **WebSocket not working**: Check Redis is running
2. **File uploads failing**: Check file size limits and permissions
3. **PowerPoint add-in not loading**: Check CORS settings and SSL
4. **Images not displaying**: Check media file serving

### Debug Mode
```python
# Enable debug logging
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'handlers': {
        'console': {
            'class': 'logging.StreamHandler',
        },
    },
    'loggers': {
        'image_upload': {
            'handlers': ['console'],
            'level': 'DEBUG',
        },
    },
}
```

## 📞 Support

The feature is designed to be self-contained and well-documented. Check:
1. `image_upload/README.md` for detailed documentation
2. API endpoints in `views.py`
3. Model definitions in `models.py`
4. PRD document for specifications

## 🎉 Success!

You now have a complete image upload feature that:
- ✅ Implements all PRD MVP requirements
- ✅ Works standalone or integrated
- ✅ Provides real-time updates
- ✅ Includes PowerPoint integration
- ✅ Supports mobile devices
- ✅ Handles file processing
- ✅ Includes security features

The feature is ready to use and can be easily merged with your main Class-Point application when you're ready!
