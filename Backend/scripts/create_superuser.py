#!/usr/bin/env python
"""
Script to create a Django superuser for admin access.
Run this script to create an admin user for the Django admin interface.
"""

import os
import sys
import django

# Add the project directory to Python path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

# Set up Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'classpoint_backend.settings')
django.setup()

from django.contrib.auth.models import User
from django.core.management.utils import get_random_secret_key


def create_superuser():
    """Create a superuser for Django admin access."""
    
    # Check if superuser already exists
    if User.objects.filter(is_superuser=True).exists():
        print("Superuser already exists!")
        existing_superusers = User.objects.filter(is_superuser=True)
        for user in existing_superusers:
            print(f"  - {user.username} ({user.email})")
        return
    
    # Create superuser
    username = input("Enter admin username (default: admin): ").strip() or "admin"
    email = input("Enter admin email (default: admin@classpoint.com): ").strip() or "admin@classpoint.com"
    password = input("Enter admin password (default: admin123): ").strip() or "admin123"
    
    try:
        user = User.objects.create_superuser(
            username=username,
            email=email,
            password=password
        )
        print(f"\n✅ Superuser created successfully!")
        print(f"   Username: {username}")
        print(f"   Email: {email}")
        print(f"   Password: {password}")
        print(f"\n🌐 Access Django Admin at: http://localhost:8000/admin/")
        
    except Exception as e:
        print(f"❌ Error creating superuser: {e}")


if __name__ == "__main__":
    create_superuser()
