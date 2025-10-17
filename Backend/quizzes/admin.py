from django.contrib import admin
from .models import Quiz


@admin.register(Quiz)
class QuizAdmin(admin.ModelAdmin):
    list_display = ['id', 'title', 'quiz_type', 'course', 'created_by', 'created_at', 'show_timer']
    list_filter = ['quiz_type', 'show_timer', 'start_with_slide', 'minimize_results_window_on_start', 
                   'allow_late_submissions', 'show_results_to_students', 'created_at']
    search_fields = ['title', 'course__name', 'created_by__username']
    readonly_fields = ['created_at']
    ordering = ['-created_at']
    
    fieldsets = (
        ('Quiz Information', {
            'fields': ('title', 'quiz_type', 'course', 'created_by')
        }),
        ('Properties', {
            'fields': ('properties',),
            'description': 'JSON field containing quiz-specific settings and content'
        }),
        ('Display Settings', {
            'fields': ('start_with_slide', 'minimize_results_window_on_start', 'show_timer')
        }),
        ('Timing & Submission', {
            'fields': ('auto_close_after_seconds', 'allow_late_submissions', 'show_results_to_students')
        }),
        ('Timestamps', {
            'fields': ('created_at',),
            'classes': ('collapse',)
        }),
    )
    
    def get_queryset(self, request):
        return super().get_queryset(request).select_related('course', 'created_by')
    
    def get_form(self, request, obj=None, **kwargs):
        form = super().get_form(request, obj, **kwargs)
        # Make properties field more user-friendly
        if 'properties' in form.base_fields:
            form.base_fields['properties'].help_text = (
                'Enter quiz properties as JSON. Examples:\n'
                'Multiple Choice: {"question_text": "What is 2+2?", "choices": [{"text": "3", "is_correct": false}, {"text": "4", "is_correct": true}], "max_choices": 1}\n'
                'Short Answer: {"question_text": "What is the capital of France?", "correct_answer": "Paris"}\n'
                'Word Cloud: {"question_text": "Name programming concepts", "max_words_per_student": 5}'
            )
        return form