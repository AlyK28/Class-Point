# ClassPoint Image Upload System

A complete image upload system that replicates the ClassPoint video functionality, supporting both web-based and native C# PowerPoint Add-in interfaces.

## 🎯 Overview

This system provides a comprehensive image upload solution for educational environments, allowing teachers to create sessions where students can upload images in response to questions or prompts. The system includes real-time submission viewing, like/star functionality, and submission management - exactly like the ClassPoint video demonstration.

## ✨ Features

### 🎓 **Core Functionality (ClassPoint Video Features)**
- ✅ **Visual Activity** - Image upload with question prompts
- ✅ **Real-time Responses** - Live display of submissions as they come in
- ✅ **Star Rating System** - Give stars to unique/outstanding images
- ✅ **Delete Inappropriate** - Remove unwanted submissions instantly
- ✅ **Search & Filter** - Find submissions by student name or caption
- ✅ **Download All** - Export all submissions as PNG files in ZIP
- ✅ **Session Management** - Create, manage, and close sessions
- ✅ **QR Code Generation** - Easy session sharing with students
- ✅ **Anonymous Uploads** - Students can upload without accounts

### 🔧 **Technical Features**
- ✅ **Dual Interface** - Both web UI and C# PowerPoint Add-in
- ✅ **JWT Authentication** - Secure user authentication
- ✅ **Thumbnail Generation** - Automatic image thumbnails for fast loading
- ✅ **File Validation** - Image type and size validation
- ✅ **CORS Support** - Cross-origin request handling
- ✅ **Real-time Updates** - Live session and submission data
- ✅ **Responsive Design** - Works on desktop and mobile

## 🚀 Quick Start

### Prerequisites
- **Python 3.8+** for Django backend
- **Visual Studio 2019+** for C# frontend (optional)
- **Modern web browser** for web frontend

### 1. Backend Setup (Django)

```bash
# Navigate to backend directory
cd /Users/ali/Desktop/imageUpload/Class-Point/Backend

# Create and activate virtual environment
python3 -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run migrations
python manage.py migrate

# Create superuser (optional)
python manage.py createsuperuser

# Start the Django server
python manage.py runserver 8000
```

**Backend will be available at:** `http://localhost:8000`

### 2. Web Frontend Setup

```bash
# Navigate to web frontend directory
cd /Users/ali/Desktop/imageUpload/Class-Point/Frontend/web-frontend

# Start the web server
python3 server.py
```

**Web Frontend will be available at:** `http://localhost:3001`

### 3. C# Frontend Setup (Optional)

```bash
# Open in Visual Studio
# Navigate to: /Users/ali/Desktop/imageUpload/Class-Point/Frontend/ClassPointAddIn

# Build and run the solution
# Set Program.cs as startup object for testing
```

## 🎮 How to Use

### Default Login Credentials
- **Username:** `admin`
- **Password:** `admin123`

### Teacher Workflow

1. **Login** to either web UI or C# application
2. **Create Session** with a name and optional question
3. **Share Session Code** with students (6-character code)
4. **Monitor Submissions** in real-time
5. **Like/Star** outstanding submissions
6. **Delete** inappropriate content
7. **Download All** submissions as ZIP file
8. **Close Session** when complete

### Student Workflow

1. **Get Session Code** from teacher
2. **Enter Session Code** in upload interface
3. **Add Name** (optional)
4. **Upload Image** by selecting file
5. **See Submission** appear in real-time

## 🌐 Web Interface

### Access: `http://localhost:3001`

**Features:**
- Modern, responsive web interface
- Real-time submission updates
- Drag-and-drop image upload
- Session management dashboard
- Statistics and analytics
- Search and filter submissions
- Bulk download functionality

**Tabs:**
- **📚 Sessions** - Create and manage sessions
- **📤 Upload** - Upload images to sessions
- **🖼️ Submissions** - View and manage submissions
- **📊 Statistics** - View session analytics

## 💻 C# PowerPoint Add-in

### Features:
- Native Windows application
- PowerPoint integration
- Session management interface
- Image upload with preview
- Submissions viewer with thumbnails
- Like and delete functionality

### Testing the C# Frontend:
1. Open the solution in Visual Studio
2. Set `Program.cs` as startup object
3. Run the application (F5)
4. Click "Test Login" to authenticate
5. Click "Open Image Upload" to access features

## 🔧 API Endpoints

### Authentication
- `POST /api/users/login/` - User login
- `POST /api/users/register/` - User registration

### Session Management
- `GET /api/image-upload/teacher/sessions/` - List teacher sessions
- `POST /api/image-upload/sessions/` - Create new session
- `GET /api/image-upload/sessions/{id}/` - Get session details
- `POST /api/image-upload/sessions/{id}/close/` - Close session

### Image Upload
- `POST /api/image-upload/sessions/{code}/submissions/` - Upload image
- `GET /api/image-upload/sessions/{code}/submissions/` - List submissions
- `POST /api/image-upload/submissions/{id}/like/` - Toggle like
- `POST /api/image-upload/submissions/{id}/delete/` - Delete submission

### Statistics & Downloads
- `GET /api/image-upload/sessions/{id}/stats/` - Get statistics
- `POST /api/image-upload/sessions/{id}/download/` - Download all submissions

## 📁 Project Structure

```
imageUpload/
├── Class-Point/
│   ├── Backend/                    # Django REST API
│   │   ├── image_upload/          # Image upload app
│   │   ├── users/                 # User management
│   │   ├── classes/               # Class management
│   │   ├── courses/               # Course management
│   │   ├── students/              # Student management
│   │   ├── quizzes/               # Quiz management
│   │   └── manage.py              # Django management
│   ├── Frontend/
│   │   ├── web-frontend/          # Web UI (HTML/JS)
│   │   │   ├── index.html         # Main web interface
│   │   │   └── server.py          # Web server
│   │   └── ClassPointAddIn/       # C# PowerPoint Add-in
│   │       ├── Api/               # API clients
│   │       ├── Views/             # UI forms
│   │       ├── Users/             # Authentication
│   │       └── ThisAddIn.cs       # Main add-in
│   └── README.md                  # This file
└── ppt-image-upload-prd.md        # Product requirements
```

## 🛠️ Development

### Backend Development
```bash
# Activate virtual environment
source venv/bin/activate

# Run development server
python manage.py runserver 8000

# Create new migrations
python manage.py makemigrations

# Apply migrations
python manage.py migrate

# Run tests
python manage.py test
```

### Frontend Development
```bash
# Web frontend
cd Frontend/web-frontend
python3 server.py

# C# frontend
# Open in Visual Studio and build
```

## 🔒 Security Features

- **JWT Authentication** - Secure token-based auth
- **File Validation** - Image type and size limits
- **CORS Protection** - Cross-origin request security
- **Input Sanitization** - XSS protection
- **File Size Limits** - 10MB maximum per image
- **Supported Formats** - JPG, PNG, GIF, WebP only

## 🐛 Troubleshooting

### Common Issues

1. **"Failed to connect to backend"**
   - Ensure Django server is running on port 8000
   - Check firewall settings
   - Verify no other service is using port 8000

2. **"Login failed"**
   - Use correct credentials: admin/admin123
   - Check Django backend logs
   - Verify user exists in database

3. **"Upload failed"**
   - Check session code is correct (6 characters)
   - Ensure session is active (not closed)
   - Verify file is an image under 10MB

4. **"Images not displaying"**
   - Check Django media file serving
   - Verify image URLs in browser network tab
   - Ensure thumbnails are generated

5. **"CORS errors"**
   - Frontend includes CORS headers
   - Access via correct URLs (localhost:3001)
   - Check browser console for specific errors

### Debug Mode

**Backend:**
```bash
# Enable debug logging
export DJANGO_DEBUG=True
python manage.py runserver 8000
```

**Frontend:**
- Open browser developer tools
- Check Network tab for API requests
- View Console for JavaScript errors

## 📊 Performance

- **Image Processing** - Automatic thumbnail generation
- **File Storage** - Local filesystem with organized structure
- **Database** - SQLite for development, PostgreSQL for production
- **Caching** - Browser caching for static assets
- **Compression** - ZIP downloads with compression

## 🚀 Production Deployment

### Backend (Django)
1. **Database** - Switch to PostgreSQL
2. **Static Files** - Use nginx or Apache
3. **Media Files** - Use cloud storage (AWS S3, etc.)
4. **Security** - Enable HTTPS, update CORS settings
5. **Environment** - Use environment variables for secrets

### Frontend
1. **Web UI** - Serve through nginx with HTTPS
2. **C# Add-in** - Package and distribute through Microsoft Store
3. **CDN** - Use CDN for static assets
4. **Monitoring** - Add logging and error tracking

## 📝 License

This project is part of the ClassPoint educational technology suite.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📞 Support

For issues and questions:
1. Check the troubleshooting section
2. Review the API documentation
3. Check Django and browser console logs
4. Create an issue with detailed information

---

**🎉 The ClassPoint Image Upload System is now fully functional with both web and native interfaces, providing all the features shown in the ClassPoint video!**
