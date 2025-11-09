# ClassPoint Student Portal

A React-based student portal for participating in live quizzes created by teachers in ClassPoint.

## Features

- 🔐 **Easy Authentication**: Students join classes using a 4-digit class code and their name
- 📝 **Multiple Quiz Types**: Support for various quiz formats
  - Multiple Choice
  - Short Answer
  - Word Cloud  
  - Drawing
  - Image Upload
- 🔄 **Real-time Updates**: Auto-refreshes every 5 seconds to show new quizzes
- 📱 **Responsive Design**: Works on desktop, tablet, and mobile devices
- ✅ **Instant Feedback**: Students see confirmation when answers are submitted

## Prerequisites

- Node.js (v16 or higher)
- npm or yarn
- Django backend running on `http://localhost:8000`

## Installation

1. Navigate to the student portal directory:
```bash
cd student-portal
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:5173`

## Usage

### For Students

1. **Join a Class**:
   - Open the student portal
   - Enter your full name
   - Enter the 4-digit class code provided by your teacher
   - Click "Join Class"

2. **View Available Quizzes**:
   - After joining, you'll see all quizzes created by your teacher
   - The page auto-refreshes to show new quizzes as they're created

3. **Submit Answers**:
   - Click on any quiz card to participate
   - Answer according to the quiz type
   - Click "Submit Answer" when done
   - You'll see a confirmation message

### For Teachers

Teachers should:
1. Create a class in the C# PowerPoint Add-in
2. Share the 4-digit class code with students
3. Create quizzes from the PowerPoint Add-in
4. Students will automatically see new quizzes appear

## Quiz Types

### Multiple Choice
- Select one or multiple answers
- Visual letter indicators (A, B, C, D...)
- Clear selection highlighting

### Short Answer
- Text-based responses
- Multi-line support
- Character limit varies by quiz

### Word Cloud
- Submit multiple words/phrases
- Tag-based display
- Easy word removal

### Drawing
- Canvas-based drawing
- Color and brush size controls
- Clear canvas option

### Image Upload
- File selection with preview
- Format and size validation
- Supported formats: JPG, PNG, JPEG

## API Integration

The portal connects to the Django backend at `http://localhost:8000/api` with the following endpoints:

- `POST /api/students/join-class/` - Join a class
- `GET /api/students/quizzes/` - Get available quizzes
- `POST /api/students/answers/` - Submit quiz answer
- `GET /api/students/submissions/` - View submissions

## Configuration

To change the API base URL, edit `src/services/api.ts`:

```typescript
const API_BASE_URL = 'http://localhost:8000/api';
```

## Building for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

## Project Structure

```
student-portal/
├── src/
│   ├── components/
│   │   ├── QuizCard.tsx
│   │   ├── QuizModal.tsx
│   │   └── quiz-types/
│   │       ├── MultipleChoiceQuiz.tsx
│   │       ├── ShortAnswerQuiz.tsx
│   │       ├── WordCloudQuiz.tsx
│   │       ├── DrawingQuiz.tsx
│   │       └── ImageUploadQuiz.tsx
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   └── QuizDashboard.tsx
│   ├── services/
│   │   └── api.ts
│   ├── styles/
│   │   ├── LoginPage.css
│   │   ├── QuizDashboard.css
│   │   ├── QuizCard.css
│   │   ├── QuizModal.css
│   │   └── QuizTypes.css
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── package.json
└── vite.config.ts
```

## Technologies Used

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **React Router** - Client-side routing
- **CSS3** - Styling with custom properties

## Troubleshooting

### Can't connect to backend
- Ensure Django server is running on port 8000
- Check CORS settings in Django
- Verify API endpoints are accessible

### Quiz not appearing
- Click the refresh button
- Check if the class is still active
- Verify the quiz was created in the correct course

### Answer submission fails
- Check internet connection
- Verify you're still authenticated (token hasn't expired)
- For file uploads, check file size and format

## License

This project is part of the ClassPoint application.
