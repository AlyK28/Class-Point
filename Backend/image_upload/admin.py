from django.contrib import admin
from .models import ImageUploadSession, ImageSubmission, SessionSettings


@admin.register(ImageUploadSession)
class ImageUploadSessionAdmin(admin.ModelAdmin):
    list_display = ['name', 'session_code', 'teacher', 'status', 'submission_count', 'created_at']
    list_filter = ['status', 'created_at', 'teacher']
    search_fields = ['name', 'session_code', 'teacher__username']
    readonly_fields = ['id', 'session_code', 'submission_count', 'created_at', 'closed_at']
    
    fieldsets = (
        ('Session Information', {
            'fields': ('name', 'question', 'session_code', 'status')
        }),
        ('Settings', {
            'fields': ('allow_anonymous', 'max_submissions', 'submission_count')
        }),
        ('Metadata', {
            'fields': ('teacher', 'created_at', 'closed_at'),
            'classes': ('collapse',)
        }),
    )


@admin.register(ImageSubmission)
class ImageSubmissionAdmin(admin.ModelAdmin):
    list_display = ['student_name', 'session', 'file_name', 'file_size', 'likes', 'uploaded_at']
    list_filter = ['uploaded_at', 'is_liked', 'session__teacher', 'mime_type']
    search_fields = ['student_name', 'file_name', 'session__name', 'session__session_code']
    readonly_fields = ['id', 'file_name', 'file_size', 'mime_type', 'uploaded_at', 'deleted_at']
    
    fieldsets = (
        ('Submission Information', {
            'fields': ('session', 'student_name', 'image', 'thumbnail')
        }),
        ('File Details', {
            'fields': ('file_name', 'file_size', 'mime_type', 'metadata')
        }),
        ('Engagement', {
            'fields': ('likes', 'is_liked')
        }),
        ('Metadata', {
            'fields': ('uploaded_at', 'deleted_at'),
            'classes': ('collapse',)
        }),
    )


@admin.register(SessionSettings)
class SessionSettingsAdmin(admin.ModelAdmin):
    list_display = ['teacher', 'max_file_size_mb', 'default_anonymous', 'updated_at']
    list_filter = ['default_anonymous', 'auto_download', 'created_at']
    search_fields = ['teacher__username']
    readonly_fields = ['created_at', 'updated_at']
    
    fieldsets = (
        ('File Settings', {
            'fields': ('max_file_size_mb', 'allowed_formats')
        }),
        ('Default Settings', {
            'fields': ('auto_download', 'default_anonymous')
        }),
        ('Metadata', {
            'fields': ('created_at', 'updated_at'),
            'classes': ('collapse',)
        }),
    )
