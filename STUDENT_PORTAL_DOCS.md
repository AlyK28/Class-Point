# ClassPoint Student Portal - Complete Documentation

## 🎯 Overview

The ClassPoint Student Portal is a React-based web application that allows students to:
- Join classes using a simple 4-digit code
- View live quizzes created by teachers
- Submit answers for various quiz types
- See real-time updates when new quizzes are created

## ✅ Current Status

### ✓ Completed Features

1. **Student Authentication System**
   - Join class with name + 4-digit code
   - JWT token-based authentication
   - 24-hour token expiration
   - Secure localStorage persistence

2. **Quiz Dashboard**
   - Auto-refresh every 5 seconds for new quizzes
   - Beautiful card-based layout
   - Responsive design for all devices
   - Class information display

3. **Quiz Submission System**
   - Multiple Choice (single/multiple selection)
   - Short Answer (text input)
   - Word Cloud (multi-word submission)
   - Drawing (canvas-based)
   - Image Upload (file upload with preview)

4. **Teacher Visibility**
   - All submissions stored in database
   - Teachers can view student answers via Django admin or API
   - Real-time submission tracking

## 🚀 How to Run

### Prerequisites
- Python 3.8+ with Django backend setup
- Node.js 16+ for React frontend
- Both servers must run simultaneously

### Step 1: Start Django Backend

```powershell
cd "d:\Class-Point\Backend"
python manage.py runserver
```

✓ Backend running at: `http://localhost:8000`

### Step 2: Start React Frontend

```powershell
cd "d:\Class-Point\student-portal"
npm run dev
```

✓ Frontend running at: `http://localhost:5173`

### Step 3: Access the Portal

Open your browser and navigate to: `http://localhost:5173`

## 📝 User Flow

### Student Flow

1. **Open Portal** → Student visits `http://localhost:5173`
2. **Enter Credentials** → Enters name and 4-digit class code
3. **Join Class** → Receives JWT token, redirected to dashboard
4. **View Quizzes** → Sees all quizzes for their class
5. **Take Quiz** → Clicks quiz card, modal opens
6. **Submit Answer** → Completes quiz, sees success message
7. **Auto-Update** → Dashboard refreshes every 5 seconds for new quizzes

### Teacher Flow (Backend)

1. **Create Class** → Using C# Add-in or Django admin
2. **Share Code** → Give 4-digit code to students
3. **Create Quiz** → Using C# Add-in
4. **View Submissions** → Check Django admin or use API endpoints
5. **Monitor** → See student answers in real-time

## 🔧 Technical Architecture

### Frontend (React + TypeScript + Vite)

```
student-portal/
├── src/
│   ├── components/           # Reusable UI components
│   │   ├── QuizCard.tsx     # Individual quiz display card
│   │   ├── QuizModal.tsx    # Modal container for quizzes
│   │   └── quiz-types/      # Quiz type-specific components
│   ├── pages/                # Page components
│   │   ├── LoginPage.tsx    # Authentication page
│   │   └── QuizDashboard.tsx # Main quiz listing
│   ├── services/             # API integration
│   │   └── api.ts           # All backend API calls
│   ├── styles/               # CSS files
│   └── App.tsx              # Main app with routing
```

### Backend (Django)

**Key Endpoints Used:**

- `POST /api/students/join-class/` 
  - Input: `{ full_name, class_code }`
  - Output: `{ access_token, student_id, class_id, ... }`

- `GET /api/students/quizzes/`
  - Headers: `Authorization: Bearer <token>`
  - Output: `{ quizzes: [...], class_info: {...} }`

- `POST /api/students/answers/`
  - Headers: `Authorization: Bearer <token>`
  - Body: `{ quiz_id, answer_data, uploaded_file? }`
  - Output: `{ id, submission, answer_data, ... }`

### Authentication Flow

```
Student Browser          React App              Django Backend
     |                       |                        |
     |--[Enter Name+Code]--->|                        |
     |                       |--[POST /join-class]--->|
     |                       |                        |--[Create Student]
     |                       |                        |--[Create Enrollment]
     |                       |                        |--[Generate JWT]
     |                       |<--[Return Token]-------|
     |                       |--[Save to localStorage]|
     |<--[Redirect to Dashboard]                      |
     |                       |                        |
     |                       |--[GET /quizzes + Token]>|
     |                       |<--[Return Quizzes]-----|
     |                       |                        |
     |--[Submit Answer]----->|--[POST /answers + Token]>|
     |<--[Success Message]---|<--[Saved]--------------|
```

## 🎨 Quiz Types Explained

### 1. Multiple Choice

**Student View:**
- Displays question text
- Shows lettered choices (A, B, C, D...)
- Single or multiple selection
- Visual selection feedback

**Answer Data Structure:**
```json
{
  "selected_choices": [0, 2],
  "choice_texts": ["Option A", "Option C"]
}
```

### 2. Short Answer

**Student View:**
- Large text area for typing
- Supports multi-line answers
- Character limit displayed

**Answer Data Structure:**
```json
{
  "answer_text": "Student's written answer here..."
}
```

### 3. Word Cloud

**Student View:**
- Input field for words/phrases
- Tag display for added words
- Remove individual words option
- Max words limit enforced

**Answer Data Structure:**
```json
{
  "words": ["python", "javascript", "react"]
}
```

### 4. Drawing

**Student View:**
- Canvas for drawing
- Color picker
- Brush size slider
- Clear canvas button

**Answer Data Structure:**
```json
{
  "drawing_type": "canvas"
}
```
+ File: `drawing.png` (uploaded separately)

### 5. Image Upload

**Student View:**
- File selector
- Image preview
- Format validation
- Size limit checking

**Answer Data Structure:**
```json
{
  "image_type": "upload"
}
```
+ File: Selected image (uploaded separately)

## 🔄 Real-Time Updates

The dashboard uses **polling** (every 5 seconds) to check for new quizzes:

```typescript
useEffect(() => {
  fetchQuizzes(); // Initial fetch
  
  const interval = setInterval(fetchQuizzes, 5000); // Poll every 5s
  
  return () => clearInterval(interval); // Cleanup
}, [token]);
```

**Alternative (Future):** WebSocket for true real-time updates

## 🎯 Teacher Integration

### Viewing Student Answers

**Option 1: Django Admin**
```
http://localhost:8000/admin
→ Students → Student answers
```

**Option 2: API Endpoint**
```python
# GET /api/students/answers/
# (Authenticated as teacher)

# Returns all answers for teacher's quizzes
```

**Option 3: Custom Teacher Dashboard**
```typescript
// Future enhancement
// Create a teacher view in React showing:
// - All student submissions
// - Answer statistics
// - Grading interface
```

## 🔐 Security

### Current Implementation

- ✓ JWT tokens for authentication
- ✓ Token expiration (24 hours)
- ✓ CORS protection
- ✓ Student-teacher isolation
- ✓ Class code validation

### Recommendations for Production

- [ ] HTTPS only
- [ ] Token refresh mechanism
- [ ] Rate limiting
- [ ] Input sanitization
- [ ] File upload restrictions
- [ ] CSRF protection

## 📊 Database Schema

### Key Models

**Student**
- `full_name`: CharField
- `email`: EmailField (optional)
- `joined_at`: DateTimeField

**StudentClassEnrollment**
- `student`: ForeignKey → Student
- `classroom`: ForeignKey → Class
- `joined_at`: DateTimeField
- Unique: (student, classroom)

**StudentQuizSubmission**
- `student`: ForeignKey → Student
- `quiz`: ForeignKey → Quiz
- `submitted_at`: DateTimeField
- `score`: DecimalField (optional)
- Unique: (student, quiz)

**StudentAnswer**
- `submission`: ForeignKey → StudentQuizSubmission
- `answer_data`: JSONField
- `uploaded_file`: FileField (optional)
- `submitted_at`: DateTimeField

## 🚧 Future Enhancements

### Short-term (Next Steps)

1. **Results Display**
   - Show quiz results to students after teacher grades
   - Display correct answers (if enabled by teacher)
   - Score visualization

2. **Timer Integration**
   - Visual countdown timer
   - Auto-submit when time expires
   - Warning when time is low

3. **Error Handling**
   - Better error messages
   - Retry mechanism for failed submissions
   - Offline detection

### Mid-term

1. **WebSocket Integration**
   - Replace polling with WebSocket
   - Instant quiz appearance
   - Live student count for teachers

2. **Mobile Optimization**
   - Touch-friendly drawing canvas
   - Better mobile layouts
   - Progressive Web App (PWA)

3. **Accessibility**
   - Screen reader support
   - Keyboard navigation
   - High contrast mode

### Long-term

1. **Analytics Dashboard**
   - Student performance tracking
   - Quiz participation rates
   - Answer pattern analysis

2. **Gamification**
   - Points and badges
   - Leaderboards
   - Streaks and achievements

3. **Advanced Quiz Types**
   - Audio responses
   - Video uploads
   - Collaborative answers

## 🐛 Troubleshooting

### Students Can't Join Class

**Check:**
- [ ] Is class code correct? (4 digits)
- [ ] Is class active? (`active=True` in database)
- [ ] Is Django server running?
- [ ] Check browser console for errors

**Fix:**
```python
# In Django shell
from classes.models import Class
cls = Class.objects.get(code='1234')
cls.active = True
cls.save()
```

### Quizzes Not Appearing

**Check:**
- [ ] Does quiz belong to correct course?
- [ ] Is student enrolled in the right class?
- [ ] Check network tab for API errors
- [ ] Try manual refresh button

**Fix:**
```python
# Verify quiz belongs to student's course
from students.models import StudentClassEnrollment
enrollment = StudentClassEnrollment.objects.get(id=X)
print(enrollment.classroom.course.id)  # Should match quiz.course.id
```

### Answer Submission Fails

**Check:**
- [ ] Is token expired? (>24 hours)
- [ ] Is file size within limits? (for uploads)
- [ ] Check Django server logs
- [ ] Verify quiz still exists

**Fix:**
```powershell
# Check Django logs for detailed error
# Usually shows validation errors or permission issues
```

### CORS Errors

**Check:**
- [ ] `django-cors-headers` installed?
- [ ] `corsheaders` in `INSTALLED_APPS`?
- [ ] Frontend URL in `CORS_ALLOWED_ORIGINS`?

**Fix:**
```python
# In settings.py
CORS_ALLOWED_ORIGINS = [
    "http://localhost:5173",
    "http://127.0.0.1:5173",
]
```

## 📱 Deployment Guide

### Frontend Deployment

1. **Build Production Bundle**
```powershell
cd student-portal
npm run build
```

2. **Deploy Options**
   - **Vercel**: `vercel deploy`
   - **Netlify**: Drag & drop `dist` folder
   - **Django Static**: Copy to Django's static folder

3. **Environment Variables**
```env
VITE_API_URL=https://your-backend.com/api
```

### Backend Deployment

1. **Update Django Settings**
```python
ALLOWED_HOSTS = ['your-domain.com']
CORS_ALLOWED_ORIGINS = ['https://your-frontend.com']
DEBUG = False
```

2. **Collect Static Files**
```powershell
python manage.py collectstatic
```

3. **Use Production Server**
```powershell
gunicorn classpoint_backend.wsgi:application
```

## 📞 Support & Resources

### Documentation
- React: https://react.dev
- Django: https://docs.djangoproject.com
- Vite: https://vitejs.dev

### Key Files
- Frontend Entry: `src/main.tsx`
- API Service: `src/services/api.ts`
- Backend Settings: `Backend/classpoint_backend/settings.py`
- Student Views: `Backend/students/views.py`

### Testing Checklist

- [ ] Student can join class
- [ ] Dashboard shows quizzes
- [ ] Multiple choice quiz works
- [ ] Short answer quiz works
- [ ] Word cloud quiz works
- [ ] Drawing quiz works
- [ ] Image upload quiz works
- [ ] Auto-refresh shows new quizzes
- [ ] Submissions appear in Django admin
- [ ] Token persistence works (refresh page)
- [ ] Logout works
- [ ] Responsive on mobile

## 🎉 Success!

You now have a fully functional student portal integrated with your Django backend. Students can join classes, participate in live quizzes, and teachers can see all submissions in real-time!

**Both servers running:**
- ✓ Django Backend: `http://localhost:8000`
- ✓ React Frontend: `http://localhost:5173`

**Next:** Share the student portal URL with your students and the class code, then create some quizzes from your C# PowerPoint Add-in!
