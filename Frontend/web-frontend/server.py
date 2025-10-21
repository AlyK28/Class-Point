#!/usr/bin/env python3
"""
Simple HTTP server to serve the ClassPoint web frontend.
This allows you to test the image upload functionality in a web browser.
"""

import http.server
import socketserver
import webbrowser
import os
import sys
from pathlib import Path

# Configuration
PORT = 3001
HOST = 'localhost'

class CustomHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    """Custom handler to serve files with proper MIME types."""
    
    def end_headers(self):
        # Add CORS headers to allow requests to Django backend
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        super().end_headers()
    
    def do_OPTIONS(self):
        # Handle preflight requests
        self.send_response(200)
        self.end_headers()

def main():
    """Start the web server."""
    # Change to the directory containing this script
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    print("🚀 Starting ClassPoint Web Frontend Server")
    print("=" * 50)
    print(f"📁 Serving files from: {script_dir}")
    print(f"🌐 Server running at: http://{HOST}:{PORT}")
    print(f"🔗 Frontend URL: http://{HOST}:{PORT}/index.html")
    print("=" * 50)
    print("📋 Instructions:")
    print("1. Make sure your Django backend is running on http://localhost:8000")
    print("2. Open your web browser and go to the URL above")
    print("3. Login with username: admin, password: admin123")
    print("4. Test the image upload functionality!")
    print("=" * 50)
    print("Press Ctrl+C to stop the server")
    print()
    
    try:
        with socketserver.TCPServer((HOST, PORT), CustomHTTPRequestHandler) as httpd:
            print(f"✅ Server started successfully on port {PORT}")
            
            # Try to open the browser automatically
            try:
                webbrowser.open(f'http://{HOST}:{PORT}/index.html')
                print("🌐 Browser opened automatically")
            except Exception as e:
                print(f"⚠️  Could not open browser automatically: {e}")
                print(f"   Please manually open: http://{HOST}:{PORT}/index.html")
            
            print("\n🔄 Server is running...")
            httpd.serve_forever()
            
    except KeyboardInterrupt:
        print("\n\n🛑 Server stopped by user")
        sys.exit(0)
    except OSError as e:
        if e.errno == 48:  # Address already in use
            print(f"❌ Error: Port {PORT} is already in use")
            print(f"   Try using a different port or stop the process using port {PORT}")
        else:
            print(f"❌ Error starting server: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
