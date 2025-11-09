# ClassPoint Student Portal Setup Guide

## Quick Start

### 1. Start the Django Backend

```powershell
cd "d:\Class-Point\Backend"

# Activate virtual environment if you have one
# venv\Scripts\Activate.ps1

# Start the Django server
python manage.py runserver
```

The backend will be available at `http://localhost:8000`

### 2. Start the React Frontend

Open a new terminal:

```powershell
cd "d:\Class-Point\student-portal"

# Start the Vite dev server
npm run dev
```

The student portal will be available at `http://localhost:5173`

### 3. Test the Flow

1. **Create a class** (using your C# PowerPoint Add-in or Django admin)
2. **Get the 4-digit class code**
3. **Open the student portal** at `http://localhost:5173`
4. **Join the class** with a student name and the class code
5. **Create a quiz** from the teacher side
6. **See the quiz appear** in the student portal (auto-refreshes every 5 seconds)
7. **Submit an answer** to test the complete flow

## Development Notes

### Backend Changes Made

1. **Added CORS support** in `settings.py`:
   - Installed `django-cors-headers`
   - Configured to allow requests from `http://localhost:5173`

2. **Existing API endpoints** used:
   - `POST /api/students/join-class/` - Returns JWT token for students
   - `GET /api/students/quizzes/` - Lists quizzes for authenticated students
   - `POST /api/students/answers/` - Submits quiz answers

### Frontend Structure

- **Login Page**: Students enter name and class code
- **Dashboard**: Shows all available quizzes with auto-refresh
- **Quiz Modal**: Interactive quiz submission for different types
- **Quiz Components**: Separate components for each quiz type

### Authentication Flow

1. Student submits name + class code
2. Backend returns JWT token
3. Token stored in localStorage
4. Token sent with all subsequent requests
5. Token expires after 24 hours

## Testing Different Quiz Types

### Multiple Choice
```python
# In Django shell or admin
from quizzes.models import Quiz
from courses.models import Course

quiz = Quiz.objects.create(
    course=course,
    title="Sample Multiple Choice",
    quiz_type="multiple_choice",
    created_by=teacher,
    properties={
        "question_text": "What is 2+2?",
        "choices": [
            {"text": "3", "is_correct": False},
            {"text": "4", "is_correct": True},
            {"text": "5", "is_correct": False}
        ],
        "allow_multiple_choices": False
    }
)
```

### Short Answer
```python
quiz = Quiz.objects.create(
    course=course,
    title="Short Answer Question",
    quiz_type="short_answer",
    created_by=teacher,
    properties={
        "question_text": "Explain photosynthesis",
        "correct_answer": "Process where plants convert light to energy",
        "expected_keywords": "light,chlorophyll,oxygen,glucose"
    }
)
```

### Word Cloud
```python
quiz = Quiz.objects.create(
    course=course,
    title="Word Association",
    quiz_type="word_cloud",
    created_by=teacher,
    properties={
        "question_text": "Name words associated with programming",
        "max_words_per_student": 3,
        "allow_duplicates": True
    }
)
```

## Deployment Considerations

### Production Build

```powershell
cd student-portal
npm run build
```

### Serve with Django

You can serve the React build from Django:

1. Build the React app
2. Copy `dist` folder contents to Django's `static` folder
3. Update Django to serve the React app

### Environment Variables

Create `.env` in student-portal:

```env
VITE_API_URL=http://your-production-domain.com/api
```

Update `api.ts`:

```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000/api';
```

## Common Issues

### CORS Errors
- Ensure `django-cors-headers` is installed
- Check `CORS_ALLOWED_ORIGINS` in settings.py
- Verify frontend is running on port 5173

### Token Expiration
- Tokens expire after 24 hours
- Students need to rejoin the class
- Consider implementing token refresh

### Quiz Not Showing
- Check if class is active
- Verify quiz belongs to the correct course
- Check browser console for errors

## Next Steps

1. **WebSocket Support**: Replace polling with WebSockets for real-time updates
2. **Results Display**: Show quiz results to students after teacher reviews
3. **Mobile App**: Convert to React Native for mobile
4. **Offline Support**: Add service workers for offline capability
5. **Analytics**: Track student participation and performance

## Support

For issues or questions:
1. Check the browser console for errors
2. Check Django server logs
3. Verify database has proper data
4. Test API endpoints directly

Happy coding! 🚀
