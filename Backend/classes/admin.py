from django.contrib import admin
from .models import Class


@admin.register(Class)
class ClassAdmin(admin.ModelAdmin):
    list_display = ['id', 'code', 'course', 'teacher', 'active', 'created_at']
    list_filter = ['active', 'created_at', 'course']
    search_fields = ['code', 'course__name', 'teacher__username']
    readonly_fields = ['code', 'created_at']
    ordering = ['-created_at']
    actions = ['deactivate_classes']
    
    fieldsets = (
        ('Class Information', {
            'fields': ('code', 'course', 'teacher')
        }),
        ('Status', {
            'fields': ('active',),
            'description': 'Only one active class per course per teacher is allowed.'
        }),
        ('Timestamps', {
            'fields': ('created_at',),
            'classes': ('collapse',)
        }),
    )
    
    def get_queryset(self, request):
        return super().get_queryset(request).select_related('course', 'teacher')
    
    def deactivate_classes(self, request, queryset):
        """Admin action to deactivate selected classes."""
        updated = queryset.update(active=False)
        self.message_user(
            request,
            f'{updated} class(es) were successfully deactivated.'
        )
    deactivate_classes.short_description = "Deactivate selected classes"
    
    def get_form(self, request, obj=None, **kwargs):
        form = super().get_form(request, obj, **kwargs)
        # Add help text for the active field
        if 'active' in form.base_fields:
            form.base_fields['active'].help_text = (
                'Only one active class per course per teacher is allowed. '
                'If you activate this class, any other active class for the same teacher and course will need to be deactivated first.'
            )
        return form