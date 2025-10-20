#!/usr/bin/env python3
"""
ClassPoint Image Upload System Test Script
Tests both backend and frontend functionality
"""

import requests
import json
import sys
import time
from pathlib import Path

# Configuration
BACKEND_URL = "http://localhost:8000"
WEB_FRONTEND_URL = "http://localhost:3001"
ADMIN_USERNAME = "admin"
ADMIN_PASSWORD = "admin123"

def test_backend_connection():
    """Test if Django backend is running"""
    print("🔍 Testing Django backend connection...")
    try:
        response = requests.get(f"{BACKEND_URL}/api/users/login/", timeout=5)
        if response.status_code in [200, 405]:  # 405 is OK for GET on POST endpoint
            print("✅ Django backend is running")
            return True
        else:
            print(f"❌ Django backend returned status {response.status_code}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ Django backend connection failed: {e}")
        return False

def test_authentication():
    """Test user authentication"""
    print("🔐 Testing authentication...")
    try:
        auth_data = {
            "username": ADMIN_USERNAME,
            "password": ADMIN_PASSWORD
        }
        response = requests.post(f"{BACKEND_URL}/api/users/login/", json=auth_data, timeout=10)
        
        if response.status_code == 200:
            data = response.json()
            if "access" in data:
                print("✅ Authentication successful")
                return data["access"]
            else:
                print("❌ No access token in response")
                return None
        else:
            print(f"❌ Authentication failed: {response.status_code}")
            return None
    except requests.exceptions.RequestException as e:
        print(f"❌ Authentication error: {e}")
        return None

def test_session_creation(token):
    """Test session creation"""
    print("📚 Testing session creation...")
    try:
        headers = {"Authorization": f"Bearer {token}"}
        session_data = {
            "name": "Test Session",
            "question": "Upload a test image",
            "allow_anonymous": True,
            "max_submissions": 100
        }
        response = requests.post(f"{BACKEND_URL}/api/image-upload/sessions/", 
                               json=session_data, headers=headers, timeout=10)
        
        if response.status_code == 201:
            data = response.json()
            session_code = data.get("session_code")
            print(f"✅ Session created successfully: {session_code}")
            return session_code
        else:
            print(f"❌ Session creation failed: {response.status_code}")
            return None
    except requests.exceptions.RequestException as e:
        print(f"❌ Session creation error: {e}")
        return None

def test_web_frontend():
    """Test if web frontend is running"""
    print("🌐 Testing web frontend connection...")
    try:
        response = requests.get(f"{WEB_FRONTEND_URL}/index.html", timeout=5)
        if response.status_code == 200:
            print("✅ Web frontend is running")
            return True
        else:
            print(f"❌ Web frontend returned status {response.status_code}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ Web frontend connection failed: {e}")
        return False

def main():
    """Run all tests"""
    print("🚀 ClassPoint Image Upload System Test")
    print("=" * 50)
    
    # Test backend
    if not test_backend_connection():
        print("\n❌ Backend tests failed. Make sure Django is running:")
        print("   cd Class-Point/Backend")
        print("   source venv/bin/activate")
        print("   python manage.py runserver 8000")
        sys.exit(1)
    
    # Test authentication
    token = test_authentication()
    if not token:
        print("\n❌ Authentication failed. Check credentials or create superuser:")
        print("   python manage.py createsuperuser")
        sys.exit(1)
    
    # Test session creation
    session_code = test_session_creation(token)
    if not session_code:
        print("\n❌ Session creation failed. Check image_upload app configuration.")
        sys.exit(1)
    
    # Test web frontend
    web_frontend_ok = test_web_frontend()
    
    print("\n" + "=" * 50)
    print("🎉 SYSTEM TEST RESULTS")
    print("=" * 50)
    print("✅ Django Backend: RUNNING")
    print("✅ Authentication: WORKING")
    print("✅ Session Creation: WORKING")
    print(f"✅ Test Session Code: {session_code}")
    
    if web_frontend_ok:
        print("✅ Web Frontend: RUNNING")
        print(f"\n🌐 Web Interface: {WEB_FRONTEND_URL}")
    else:
        print("❌ Web Frontend: NOT RUNNING")
        print("\n💡 To start web frontend:")
        print("   cd Class-Point/Frontend/web-frontend")
        print("   python3 server.py")
    
    print(f"\n🔗 Backend API: {BACKEND_URL}")
    print(f"📝 Test Session Code: {session_code}")
    print("\n🎯 Ready to test image upload functionality!")
    print("\n📋 Next Steps:")
    print("1. Open web interface in browser")
    print("2. Login with admin/admin123")
    print("3. Use the test session code to upload images")
    print("4. Test all features: like, delete, download")

if __name__ == "__main__":
    main()
