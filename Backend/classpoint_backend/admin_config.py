"""
Django Admin Configuration for ClassPoint
Customizes the admin interface for better user experience.
"""

from django.contrib import admin
from django.contrib.auth.admin import UserAdmin as BaseUserAdmin
from django.contrib.auth.models import User
from django.utils.html import format_html
from django.urls import reverse
from django.utils.safestring import mark_safe


class ClassPointAdminSite(admin.AdminSite):
    """Custom admin site for ClassPoint."""
    
    site_header = "ClassPoint Administration"
    site_title = "ClassPoint Admin"
    index_title = "Welcome to ClassPoint Administration"
    
    def index(self, request, extra_context=None):
        """Custom admin index page."""
        extra_context = extra_context or {}
        extra_context.update({
            'app_list': self.get_app_list(request),
            'title': self.index_title,
            'subtitle': None,
            'has_permission': self.has_permission(request),
        })
        return super().index(request, extra_context)


# Customize User Admin
class UserAdmin(BaseUserAdmin):
    """Custom User admin with better display."""
    
    list_display = ('username', 'email', 'first_name', 'last_name', 'is_staff', 'is_active', 'date_joined')
    list_filter = ('is_staff', 'is_superuser', 'is_active', 'date_joined')
    search_fields = ('username', 'first_name', 'last_name', 'email')
    ordering = ('-date_joined',)


# Register custom admin configurations
admin.site.unregister(User)
admin.site.register(User, UserAdmin)

# Customize admin site
admin.site.site_header = "ClassPoint Administration"
admin.site.site_title = "ClassPoint Admin"
admin.site.index_title = "Welcome to ClassPoint Administration"
