# ClassPoint Web Frontend

A modern web-based frontend for testing the ClassPoint image upload functionality.

## 🚀 Quick Start

### Prerequisites
- Python 3.6+ installed
- Django backend running on `http://localhost:8000`

### Running the Frontend

1. **Start the Django backend** (if not already running):
   ```bash
   cd /Users/ali/Desktop/imageUpload/Class-Point/Backend
   source venv/bin/activate
   python manage.py runserver 8000
   ```

2. **Start the web frontend**:
   ```bash
   cd /Users/ali/Desktop/imageUpload/Class-Point/Frontend/web-frontend
   python3 server.py
   ```

3. **Open your browser** and go to: `http://localhost:3000`

## 🎯 Features

### For Teachers:
- **Login System**: Authenticate with your Django backend
- **Session Management**: Create, view, and close image upload sessions
- **QR Code Generation**: Share sessions easily with students
- **Statistics Dashboard**: View submission analytics and metrics
- **Like System**: Like and manage student submissions

### For Students:
- **Easy Upload**: Drag and drop or click to upload images
- **Session Access**: Use session codes to upload to specific sessions
- **Anonymous Uploads**: Upload without creating an account
- **Real-time Feedback**: See your uploads immediately

## 🔧 How to Use

### Teacher Workflow:
1. **Login** with your credentials (default: admin/admin123)
2. **Create a Session** with a name and optional question
3. **Share the Session Code** with students (or use the QR code)
4. **Monitor Submissions** in real-time
5. **View Statistics** and like submissions
6. **Close the Session** when done

### Student Workflow:
1. **Get the Session Code** from your teacher
2. **Enter the Session Code** in the upload tab
3. **Add your name** (optional)
4. **Upload an Image** by dragging and dropping or clicking
5. **See your submission** appear in the submissions list

## 🛠️ Technical Details

- **Frontend**: Pure HTML, CSS, JavaScript (no frameworks)
- **Backend API**: Django REST Framework
- **File Upload**: Multipart form data with drag & drop support
- **Authentication**: JWT tokens
- **CORS**: Enabled for cross-origin requests
- **Responsive Design**: Works on desktop and mobile

## 📱 Browser Compatibility

- Chrome 60+
- Firefox 55+
- Safari 12+
- Edge 79+

## 🔒 Security Features

- JWT token authentication
- File type validation (images only)
- File size limits (10MB max)
- CORS protection
- Input sanitization

## 🐛 Troubleshooting

### Common Issues:

1. **"Failed to load sessions"**
   - Make sure Django backend is running on port 8000
   - Check your login credentials

2. **"Upload failed"**
   - Verify the session code is correct
   - Check file size (must be under 10MB)
   - Ensure file is an image (JPG, PNG, GIF, WebP)

3. **"CORS error"**
   - The frontend server includes CORS headers
   - Make sure you're accessing via `http://localhost:3000`

4. **Images not displaying**
   - Check that Django media files are being served
   - Verify the image URLs in the browser network tab

### Development Tips:

- Open browser developer tools to see API requests
- Check the Django server logs for backend errors
- Use the browser's network tab to debug upload issues

## 🎨 Customization

The frontend is built with vanilla HTML/CSS/JavaScript, making it easy to customize:

- **Styling**: Modify the CSS in the `<style>` section
- **API Endpoints**: Update the `API_BASE` constant
- **Features**: Add new functionality in the JavaScript section
- **UI Components**: Extend the HTML structure

## 📊 API Integration

The frontend integrates with these Django API endpoints:

- `POST /api/users/login/` - User authentication
- `GET /api/image-upload/teacher/sessions/` - List teacher sessions
- `POST /api/image-upload/sessions/` - Create new session
- `POST /api/image-upload/sessions/{code}/submissions/` - Upload image
- `GET /api/image-upload/sessions/{code}/submissions/` - List submissions
- `POST /api/image-upload/submissions/{id}/like/` - Toggle like
- `GET /api/image-upload/sessions/{id}/stats/` - Get statistics

## 🚀 Production Deployment

For production use:

1. **Serve static files** through a proper web server (nginx, Apache)
2. **Enable HTTPS** for secure file uploads
3. **Configure CORS** properly for your domain
4. **Set up proper authentication** and user management
5. **Implement file storage** (AWS S3, Google Cloud, etc.)
6. **Add rate limiting** and security headers

## 📝 License

This frontend is part of the ClassPoint project and follows the same license terms.
