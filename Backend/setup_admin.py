#!/usr/bin/env python
"""
Quick setup script for Django admin.
This script will create a superuser and set up the admin interface.
"""

import os
import sys
import django

# Add the project directory to Python path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

# Set up Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'classpoint_backend.settings')
django.setup()

from django.core.management import execute_from_command_line


def setup_admin():
    """Set up Django admin interface."""
    
    print("🚀 Setting up Django Admin for ClassPoint...")
    print("=" * 50)
    
    # Run migrations first
    print("📦 Running migrations...")
    execute_from_command_line(['manage.py', 'migrate'])
    
    # Create superuser
    print("\n👤 Creating superuser...")
    execute_from_command_line(['manage.py', 'setup_admin'])
    
    print("\n✅ Admin setup complete!")
    print("🌐 Access Django Admin at: http://localhost:8000/admin/")
    print("📚 Available models:")
    print("   • Users (Django built-in)")
    print("   • Courses")
    print("   • Classes")
    print("   • Quizzes")
    print("   • Students")
    print("   • Student Class Enrollments")
    print("   • Student Quiz Submissions")
    print("   • Student Answers")


if __name__ == "__main__":
    setup_admin()
