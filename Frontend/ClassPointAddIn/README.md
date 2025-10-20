# ClassPoint Image Upload - C# Frontend

This is the C# PowerPoint Add-in frontend for the ClassPoint Image Upload system. It provides a native Windows application interface for managing image upload sessions and viewing submissions.

## Features

### ✅ **Complete Image Upload System**
- **Session Management**: Create, view, and close image upload sessions
- **Image Upload**: Upload images with student names to active sessions
- **Submissions Viewer**: View all submissions with like/delete functionality
- **Authentication**: Secure login with JWT tokens
- **Real-time Updates**: Live session and submission data

### 🎯 **ClassPoint Video Features Implemented**
- ✅ **Visual Activity** - Image upload with question prompts
- ✅ **Real-time Responses** - Live display of submissions
- ✅ **Star System** - Like submissions (star functionality)
- ✅ **Delete Inappropriate** - Remove unwanted submissions
- ✅ **Session Management** - Create, manage, and close sessions
- ✅ **Student Interface** - Upload images with captions

## Prerequisites

1. **Django Backend Running**: Make sure the Django backend is running on `http://localhost:8000`
2. **Visual Studio**: For building and running the C# application
3. **.NET Framework**: The project targets .NET Framework 4.7.2 or higher

## How to Run

### Option 1: Test Application (Recommended for Testing)
1. Open the solution in Visual Studio
2. Set `Program.cs` as the startup object
3. Run the application (F5)
4. Click "Test Login" to authenticate with `admin`/`admin123`
5. Click "Open Image Upload" to access the full functionality

### Option 2: PowerPoint Add-in
1. Build the solution in Release mode
2. Install the add-in in PowerPoint
3. The add-in will automatically initialize when PowerPoint starts

## Usage Guide

### 1. **Authentication**
- Use credentials: `admin` / `admin123`
- The system automatically handles JWT token management

### 2. **Session Management**
- **Create Session**: Click "Create Session" to make a new image upload session
- **View Sessions**: All active sessions are displayed in the list
- **Session Details**: Shows session code, submission count, and creation date

### 3. **Image Upload**
- **Select Session**: Choose an active session from the list
- **Choose Image**: Click "Browse..." to select an image file
- **Student Name**: Optionally enter the student's name
- **Upload**: Click "Upload Image" to submit

### 4. **View Submissions**
- **Load Submissions**: Click "View Submissions" for the selected session
- **Like Submissions**: Click the heart button to like submissions
- **Delete Submissions**: Click the trash button to remove inappropriate content
- **Image Preview**: All images are displayed with thumbnails

## API Integration

The C# frontend communicates with the Django backend through REST APIs:

- **Authentication**: `/api/users/login/`
- **Session Management**: `/api/image-upload/sessions/`
- **Image Upload**: `/api/image-upload/sessions/{code}/submissions/`
- **Submissions**: `/api/image-upload/sessions/{code}/submissions/`
- **Like/Delete**: `/api/image-upload/submissions/{id}/like/` and `/delete/`

## File Structure

```
ClassPointAddIn/
├── Api/
│   ├── Service/
│   │   ├── IImageUploadApiClient.cs
│   │   ├── ImageUploadApiClient.cs
│   │   ├── IUserApiClient.cs
│   │   └── UserApiClient.cs
│   └── Responses/
│       ├── SessionResponse.cs
│       ├── SubmissionResponse.cs
│       ├── LoginResponse.cs
│       └── RegisterResponse.cs
├── Users/
│   ├── Auth/
│   │   └── AuthenticationService.cs
│   ├── Entities/
│   │   └── User.cs
│   └── ValueObjects/
│       └── JWTToken.cs
├── Views/
│   ├── ImageUploadForm.cs
│   ├── ImageUploadForm.Designer.cs
│   ├── SubmissionsViewerForm.cs
│   ├── SubmissionsViewerForm.Designer.cs
│   ├── LoginForm.cs
│   ├── LoginForm.Designer.cs
│   ├── TestForm.cs
│   └── TestForm.Designer.cs
├── ThisAddIn.cs
├── Program.cs
└── README.md
```

## Testing

### Test the Complete Workflow:
1. **Start Django Backend**: `python manage.py runserver 8000`
2. **Run C# Test App**: Use the TestForm to verify functionality
3. **Create Session**: Make a new image upload session
4. **Upload Images**: Test image upload with different file types
5. **View Submissions**: Verify submissions display correctly
6. **Test Features**: Like, delete, and manage submissions

### Expected Behavior:
- ✅ Login with admin credentials works
- ✅ Sessions are created and displayed
- ✅ Images upload successfully
- ✅ Submissions show with thumbnails
- ✅ Like and delete functions work
- ✅ Real-time updates function properly

## Troubleshooting

### Common Issues:
1. **Connection Error**: Ensure Django backend is running on port 8000
2. **Authentication Failed**: Check username/password (admin/admin123)
3. **Image Upload Fails**: Verify image file is valid and session is active
4. **Submissions Not Loading**: Check session code and network connection

### Debug Mode:
- Enable debug logging in Visual Studio
- Check network requests in browser developer tools
- Verify Django backend logs for API calls

## Integration with PowerPoint

The add-in integrates with PowerPoint through:
- **Ribbon Interface**: Custom buttons in PowerPoint ribbon
- **Event Handling**: Responds to presentation events
- **Modal Dialogs**: Shows forms as modal dialogs over PowerPoint

## Next Steps

For production deployment:
1. **Ribbon Customization**: Add custom ribbon XML for PowerPoint
2. **Error Handling**: Implement comprehensive error handling
3. **Logging**: Add proper logging and monitoring
4. **Configuration**: Make API endpoints configurable
5. **Packaging**: Create proper installer for the add-in

---

**The C# frontend now provides the same functionality as the web frontend, ensuring that image upload features work in both the basic UI (C# PowerPoint Add-in) and the web UI!** 🎉
