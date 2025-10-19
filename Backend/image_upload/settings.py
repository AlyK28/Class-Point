# Image Upload App Settings
# Add these settings to your main Django settings.py file

# Image Upload Configuration
IMAGE_UPLOAD_SETTINGS = {
    # File upload settings
    'MAX_FILE_SIZE_MB': 10,
    'ALLOWED_IMAGE_FORMATS': ['jpg', 'jpeg', 'png', 'gif', 'webp'],
    'THUMBNAIL_SIZE': (200, 200),
    
    # Session settings
    'DEFAULT_MAX_SUBMISSIONS': 100,
    'SESSION_CODE_LENGTH': 6,
    'SESSION_EXPIRY_HOURS': 24,
    
    # Storage settings
    'USE_S3': False,  # Set to True for production
    'S3_BUCKET_NAME': 'your-bucket-name',
    'S3_REGION': 'us-east-1',
    
    # CDN settings
    'USE_CDN': False,  # Set to True for production
    'CDN_DOMAIN': 'https://your-cdn-domain.com',
    
    # WebSocket settings
    'WEBSOCKET_ENABLED': True,
    'WEBSOCKET_HEARTBEAT_INTERVAL': 30,
}

# Media files configuration
MEDIA_URL = '/media/'
MEDIA_ROOT = os.path.join(BASE_DIR, 'media')

# Static files configuration
STATIC_URL = '/static/'
STATIC_ROOT = os.path.join(BASE_DIR, 'staticfiles')

# File storage configuration
if IMAGE_UPLOAD_SETTINGS['USE_S3']:
    DEFAULT_FILE_STORAGE = 'storages.backends.s3boto3.S3Boto3Storage'
    STATICFILES_STORAGE = 'storages.backends.s3boto3.S3StaticStorage'
    
    AWS_ACCESS_KEY_ID = os.getenv('AWS_ACCESS_KEY_ID')
    AWS_SECRET_ACCESS_KEY = os.getenv('AWS_SECRET_ACCESS_KEY')
    AWS_STORAGE_BUCKET_NAME = IMAGE_UPLOAD_SETTINGS['S3_BUCKET_NAME']
    AWS_S3_REGION_NAME = IMAGE_UPLOAD_SETTINGS['S3_REGION']
    AWS_S3_CUSTOM_DOMAIN = IMAGE_UPLOAD_SETTINGS.get('CDN_DOMAIN', '')
    AWS_DEFAULT_ACL = 'public-read'
    AWS_S3_OBJECT_PARAMETERS = {
        'CacheControl': 'max-age=86400',
    }

# Channels configuration for WebSocket support
if IMAGE_UPLOAD_SETTINGS['WEBSOCKET_ENABLED']:
    ASGI_APPLICATION = 'classpoint_backend.asgi.application'
    
    CHANNEL_LAYERS = {
        'default': {
            'BACKEND': 'channels_redis.core.RedisChannelLayer',
            'CONFIG': {
                "hosts": [('127.0.0.1', 6379)],
            },
        },
    }

# CORS settings for cross-origin requests
CORS_ALLOWED_ORIGINS = [
    "https://localhost:3000",  # PowerPoint add-in
    "https://localhost:8000",  # Development server
]

CORS_ALLOW_CREDENTIALS = True

# Security settings
SECURE_BROWSER_XSS_FILTER = True
SECURE_CONTENT_TYPE_NOSNIFF = True
X_FRAME_OPTIONS = 'DENY'

# File upload security
FILE_UPLOAD_MAX_MEMORY_SIZE = 10 * 1024 * 1024  # 10MB
DATA_UPLOAD_MAX_MEMORY_SIZE = 10 * 1024 * 1024  # 10MB
