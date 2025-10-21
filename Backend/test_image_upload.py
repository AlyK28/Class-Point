#!/usr/bin/env python3
"""
Test script for Image Upload functionality in ClassPoint Backend.

This script tests all the image upload API endpoints to ensure they work correctly.
Run this script after starting the Django development server.

Usage:
    python test_image_upload.py

Make sure the Django server is running on http://localhost:8000
"""

import requests
import json
import os
import tempfile
from PIL import Image
import io

# Configuration
BASE_URL = "http://localhost:8000"
API_BASE = f"{BASE_URL}/api/image-upload"

# Test credentials
ADMIN_USERNAME = "admin"
ADMIN_PASSWORD = "admin123"

class ImageUploadTester:
    def __init__(self):
        self.session = requests.Session()
        self.auth_token = None
        self.test_session_id = None
        self.test_submission_id = None
        
    def log(self, message, status="INFO"):
        """Log messages with status."""
        status_colors = {
            "INFO": "\033[94m",    # Blue
            "SUCCESS": "\033[92m", # Green
            "ERROR": "\033[91m",   # Red
            "WARNING": "\033[93m"  # Yellow
        }
        color = status_colors.get(status, "")
        reset = "\033[0m"
        print(f"{color}[{status}]{reset} {message}")
    
    def create_test_image(self, filename="test_image.jpg", size=(800, 600)):
        """Create a test image file."""
        # Create a simple test image
        img = Image.new('RGB', size, color='red')
        
        # Add some text to make it more interesting
        from PIL import ImageDraw, ImageFont
        draw = ImageDraw.Draw(img)
        
        # Try to use a default font, fallback to basic if not available
        try:
            font = ImageFont.truetype("/System/Library/Fonts/Arial.ttf", 40)
        except:
            font = ImageFont.load_default()
        
        draw.text((50, 50), "Test Image", fill='white', font=font)
        draw.text((50, 100), f"Size: {size[0]}x{size[1]}", fill='white', font=font)
        
        # Save to temporary file
        temp_file = tempfile.NamedTemporaryFile(delete=False, suffix='.jpg')
        img.save(temp_file.name, 'JPEG')
        temp_file.close()
        
        return temp_file.name
    
    def authenticate(self):
        """Authenticate and get JWT token."""
        self.log("Authenticating with admin credentials...")
        
        # Get JWT token using the correct endpoint
        auth_url = f"{BASE_URL}/api/users/login/"
        auth_data = {
            "username": ADMIN_USERNAME,
            "password": ADMIN_PASSWORD
        }
        
        try:
            response = self.session.post(auth_url, json=auth_data)
            if response.status_code == 200:
                data = response.json()
                self.auth_token = data.get('access')
                if self.auth_token:
                    self.session.headers.update({
                        'Authorization': f'Bearer {self.auth_token}'
                    })
                    self.log("Authentication successful!", "SUCCESS")
                    return True
                else:
                    self.log("No access token in response", "ERROR")
                    return False
            else:
                self.log(f"Authentication failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Authentication error: {str(e)}", "ERROR")
            return False
    
    def test_create_session(self):
        """Test creating an image upload session."""
        self.log("Testing session creation...")
        
        session_data = {
            "name": "Test Image Upload Session",
            "question": "Please upload a test image for this session",
            "allow_anonymous": True,
            "max_submissions": 10
        }
        
        try:
            response = self.session.post(f"{API_BASE}/sessions/", json=session_data)
            if response.status_code == 201:
                data = response.json()
                self.test_session_id = data['id']
                self.log(f"Session created successfully! ID: {self.test_session_id}", "SUCCESS")
                self.log(f"Session Code: {data['session_code']}", "SUCCESS")
                self.log(f"Public URL: {data['public_url']}", "SUCCESS")
                return True
            else:
                self.log(f"Session creation failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Session creation error: {str(e)}", "ERROR")
            return False
    
    def test_get_sessions(self):
        """Test getting all sessions."""
        self.log("Testing session listing...")
        
        try:
            response = self.session.get(f"{API_BASE}/teacher/sessions/")
            if response.status_code == 200:
                data = response.json()
                self.log(f"Found {len(data)} sessions", "SUCCESS")
                return True
            else:
                self.log(f"Session listing failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Session listing error: {str(e)}", "ERROR")
            return False
    
    def test_public_session_view(self):
        """Test public session view (no authentication required)."""
        if not self.test_session_id:
            self.log("No test session ID available", "WARNING")
            return False
        
        self.log("Testing public session view...")
        
        # Get session details first
        try:
            response = self.session.get(f"{API_BASE}/sessions/{self.test_session_id}/")
            if response.status_code == 200:
                session_data = response.json()
                session_code = session_data['session_code']
                
                # Test public view
                public_response = requests.get(f"{API_BASE}/public/session/{session_code}/")
                if public_response.status_code == 200:
                    self.log("Public session view works!", "SUCCESS")
                    return True
                else:
                    self.log(f"Public session view failed: {public_response.status_code}", "ERROR")
                    return False
            else:
                self.log(f"Could not get session details: {response.status_code}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Public session view error: {str(e)}", "ERROR")
            return False
    
    def test_image_upload(self):
        """Test uploading an image to the session."""
        if not self.test_session_id:
            self.log("No test session ID available", "WARNING")
            return False
        
        self.log("Testing image upload...")
        
        # Get session code
        try:
            response = self.session.get(f"{API_BASE}/sessions/{self.test_session_id}/")
            if response.status_code == 200:
                session_data = response.json()
                session_code = session_data['session_code']
            else:
                self.log(f"Could not get session code: {response.status_code}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Error getting session code: {str(e)}", "ERROR")
            return False
        
        # Create test image
        test_image_path = self.create_test_image()
        
        try:
            # Prepare file upload
            with open(test_image_path, 'rb') as f:
                files = {'image': ('test_image.jpg', f, 'image/jpeg')}
                data = {
                    'student_name': 'Test Student',
                    'session': self.test_session_id
                }
                
                # Upload image (this should work without authentication for public sessions)
                upload_response = requests.post(
                    f"{API_BASE}/sessions/{session_code}/submissions/",
                    files=files,
                    data=data
                )
                
                if upload_response.status_code == 201:
                    submission_data = upload_response.json()
                    self.test_submission_id = submission_data['id']
                    self.log("Image upload successful!", "SUCCESS")
                    self.log(f"Submission ID: {self.test_submission_id}", "SUCCESS")
                    self.log(f"Image URL: {submission_data.get('image_url', 'N/A')}", "SUCCESS")
                    self.log(f"Thumbnail URL: {submission_data.get('thumbnail_url', 'N/A')}", "SUCCESS")
                    return True
                else:
                    self.log(f"Image upload failed: {upload_response.status_code} - {upload_response.text}", "ERROR")
                    return False
        except Exception as e:
            self.log(f"Image upload error: {str(e)}", "ERROR")
            return False
        finally:
            # Clean up test image
            if os.path.exists(test_image_path):
                os.unlink(test_image_path)
    
    def test_get_submissions(self):
        """Test getting submissions for a session."""
        if not self.test_session_id:
            self.log("No test session ID available", "WARNING")
            return False
        
        self.log("Testing submission listing...")
        
        try:
            response = self.session.get(f"{API_BASE}/sessions/{self.test_session_id}/")
            if response.status_code == 200:
                session_data = response.json()
                session_code = session_data['session_code']
                
                # Get submissions
                submissions_response = self.session.get(f"{API_BASE}/sessions/{session_code}/submissions/")
                if submissions_response.status_code == 200:
                    submissions_data = submissions_response.json()
                    self.log(f"Found {len(submissions_data)} submissions", "SUCCESS")
                    return True
                else:
                    self.log(f"Submissions listing failed: {submissions_response.status_code}", "ERROR")
                    return False
            else:
                self.log(f"Could not get session details: {response.status_code}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Submissions listing error: {str(e)}", "ERROR")
            return False
    
    def test_toggle_like(self):
        """Test toggling like on a submission."""
        if not self.test_submission_id:
            self.log("No test submission ID available", "WARNING")
            return False
        
        self.log("Testing like toggle...")
        
        try:
            response = self.session.post(f"{API_BASE}/submissions/{self.test_submission_id}/like/")
            if response.status_code == 200:
                data = response.json()
                self.log(f"Like toggled successfully! Likes: {data.get('likes', 0)}", "SUCCESS")
                return True
            else:
                self.log(f"Like toggle failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Like toggle error: {str(e)}", "ERROR")
            return False
    
    def test_session_stats(self):
        """Test getting session statistics."""
        if not self.test_session_id:
            self.log("No test session ID available", "WARNING")
            return False
        
        self.log("Testing session statistics...")
        
        try:
            response = self.session.get(f"{API_BASE}/sessions/{self.test_session_id}/stats/")
            if response.status_code == 200:
                data = response.json()
                self.log("Session statistics retrieved successfully!", "SUCCESS")
                self.log(f"Total submissions: {data.get('total_submissions', 0)}", "SUCCESS")
                self.log(f"Total likes: {data.get('total_likes', 0)}", "SUCCESS")
                return True
            else:
                self.log(f"Session stats failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Session stats error: {str(e)}", "ERROR")
            return False
    
    def test_close_session(self):
        """Test closing a session."""
        if not self.test_session_id:
            self.log("No test session ID available", "WARNING")
            return False
        
        self.log("Testing session closure...")
        
        try:
            response = self.session.post(f"{API_BASE}/sessions/{self.test_session_id}/close/")
            if response.status_code == 200:
                data = response.json()
                self.log("Session closed successfully!", "SUCCESS")
                self.log(f"Session status: {data.get('status', 'unknown')}", "SUCCESS")
                return True
            else:
                self.log(f"Session closure failed: {response.status_code} - {response.text}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Session closure error: {str(e)}", "ERROR")
            return False
    
    def test_settings(self):
        """Test session settings."""
        self.log("Testing session settings...")
        
        try:
            # Get current settings
            response = self.session.get(f"{API_BASE}/settings/")
            if response.status_code == 200:
                data = response.json()
                self.log("Settings retrieved successfully!", "SUCCESS")
                
                # Update settings
                update_data = {
                    "max_file_size_mb": 15,
                    "allowed_formats": ["jpg", "png", "gif"],
                    "default_anonymous": True
                }
                
                update_response = self.session.put(f"{API_BASE}/settings/", json=update_data)
                if update_response.status_code == 200:
                    self.log("Settings updated successfully!", "SUCCESS")
                    return True
                else:
                    self.log(f"Settings update failed: {update_response.status_code}", "ERROR")
                    return False
            else:
                self.log(f"Settings retrieval failed: {response.status_code}", "ERROR")
                return False
        except Exception as e:
            self.log(f"Settings error: {str(e)}", "ERROR")
            return False
    
    def run_all_tests(self):
        """Run all tests."""
        self.log("=" * 60, "INFO")
        self.log("Starting Image Upload API Tests", "INFO")
        self.log("=" * 60, "INFO")
        
        tests = [
            ("Authentication", self.authenticate),
            ("Create Session", self.test_create_session),
            ("Get Sessions", self.test_get_sessions),
            ("Public Session View", self.test_public_session_view),
            ("Image Upload", self.test_image_upload),
            ("Get Submissions", self.test_get_submissions),
            ("Toggle Like", self.test_toggle_like),
            ("Session Stats", self.test_session_stats),
            ("Session Settings", self.test_settings),
            ("Close Session", self.test_close_session),
        ]
        
        passed = 0
        total = len(tests)
        
        for test_name, test_func in tests:
            self.log(f"\n--- {test_name} ---", "INFO")
            try:
                if test_func():
                    passed += 1
                else:
                    self.log(f"{test_name} FAILED", "ERROR")
            except Exception as e:
                self.log(f"{test_name} ERROR: {str(e)}", "ERROR")
        
        self.log("\n" + "=" * 60, "INFO")
        self.log(f"Test Results: {passed}/{total} tests passed", "SUCCESS" if passed == total else "WARNING")
        self.log("=" * 60, "INFO")
        
        if passed == total:
            self.log("All tests passed! Image upload functionality is working correctly.", "SUCCESS")
        else:
            self.log(f"{total - passed} tests failed. Please check the errors above.", "ERROR")
        
        return passed == total

def main():
    """Main function to run the tests."""
    print("Image Upload API Test Suite")
    print("Make sure the Django server is running on http://localhost:8000")
    print("Press Enter to continue or Ctrl+C to cancel...")
    
    try:
        input()
    except KeyboardInterrupt:
        print("\nTest cancelled.")
        return
    
    tester = ImageUploadTester()
    success = tester.run_all_tests()
    
    if success:
        print("\n🎉 All tests passed! Your image upload functionality is working correctly.")
    else:
        print("\n❌ Some tests failed. Please check the Django server and try again.")

if __name__ == "__main__":
    main()
