from django.contrib import admin
from .models import Student, StudentClassEnrollment, StudentQuizSubmission, StudentAnswer


@admin.register(Student)
class StudentAdmin(admin.ModelAdmin):
    list_display = ['id', 'full_name', 'email', 'joined_at']
    list_filter = ['joined_at']
    search_fields = ['full_name', 'email']
    readonly_fields = ['joined_at']
    ordering = ['-joined_at']
    
    fieldsets = (
        ('Student Information', {
            'fields': ('full_name', 'email')
        }),
        ('Timestamps', {
            'fields': ('joined_at',),
            'classes': ('collapse',)
        }),
    )


@admin.register(StudentClassEnrollment)
class StudentClassEnrollmentAdmin(admin.ModelAdmin):
    list_display = ['id', 'student', 'classroom', 'joined_at']
    list_filter = ['joined_at', 'classroom__course']
    search_fields = ['student__full_name', 'student__email', 'classroom__code', 'classroom__course__name']
    readonly_fields = ['joined_at']
    ordering = ['-joined_at']
    
    fieldsets = (
        ('Enrollment Information', {
            'fields': ('student', 'classroom')
        }),
        ('Timestamps', {
            'fields': ('joined_at',),
            'classes': ('collapse',)
        }),
    )
    
    def get_queryset(self, request):
        return super().get_queryset(request).select_related('student', 'classroom', 'classroom__course')


@admin.register(StudentQuizSubmission)
class StudentQuizSubmissionAdmin(admin.ModelAdmin):
    list_display = ['id', 'student', 'quiz', 'score', 'is_late', 'submitted_at']
    list_filter = ['is_late', 'submitted_at', 'quiz__quiz_type', 'quiz__course']
    search_fields = ['student__full_name', 'quiz__title', 'quiz__course__name']
    readonly_fields = ['submitted_at']
    ordering = ['-submitted_at']
    
    fieldsets = (
        ('Submission Information', {
            'fields': ('student', 'quiz')
        }),
        ('Results', {
            'fields': ('score', 'is_late')
        }),
        ('Timestamps', {
            'fields': ('submitted_at',),
            'classes': ('collapse',)
        }),
    )
    
    def get_queryset(self, request):
        return super().get_queryset(request).select_related('student', 'quiz', 'quiz__course')


@admin.register(StudentAnswer)
class StudentAnswerAdmin(admin.ModelAdmin):
    list_display = ['id', 'submission', 'quiz_type', 'answer_preview', 'submitted_at']
    list_filter = ['submitted_at', 'submission__quiz__quiz_type', 'submission__quiz__course']
    search_fields = ['submission__student__full_name', 'submission__quiz__title', 'submission__quiz__course__name']
    readonly_fields = ['submitted_at']
    ordering = ['-submitted_at']
    
    fieldsets = (
        ('Answer Information', {
            'fields': ('submission',)
        }),
        ('Answer Data', {
            'fields': ('answer_data', 'uploaded_file'),
            'description': 'Answer data stored as JSON, file uploads for drawing/image questions'
        }),
        ('Timestamps', {
            'fields': ('submitted_at',),
            'classes': ('collapse',)
        }),
    )
    
    def get_queryset(self, request):
        return super().get_queryset(request).select_related(
            'submission__student', 'submission__quiz', 'submission__quiz__course'
        )
    
    def quiz_type(self, obj):
        return obj.submission.quiz.quiz_type
    quiz_type.short_description = 'Quiz Type'
    
    def answer_preview(self, obj):
        """Show a preview of the answer data"""
        if obj.answer_data:
            if 'answer_text' in obj.answer_data:
                text = obj.answer_data['answer_text']
                return text[:50] + '...' if len(text) > 50 else text
            elif 'selected_choice_indices' in obj.answer_data:
                indices = obj.answer_data['selected_choice_indices']
                return f"Selected choices: {indices}"
        return "No answer data"
    answer_preview.short_description = 'Answer Preview'
    
    def get_form(self, request, obj=None, **kwargs):
        form = super().get_form(request, obj, **kwargs)
        # Make answer_data field more user-friendly
        if 'answer_data' in form.base_fields:
            form.base_fields['answer_data'].help_text = (
                'Answer data stored as JSON. Contains quiz-specific answer information:\n'
                '• Short Answer/Word Cloud: {"answer_text": "student answer"}\n'
                '• Multiple Choice: {"selected_choice_indices": [0, 2]}\n'
                '• Drawing/Image: {"metadata": "..."}'
            )
        return form