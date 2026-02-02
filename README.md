# Class-Point

A comprehensive educational platform designed to streamline classroom management, course delivery, and student engagement. Class-Point consists of three main components: a Django REST API backend, a C# Office Add-in for Microsoft Office integration, and a React-based student portal.

## 📋 Project Structure

### Backend (`/Backend`)
Django REST Framework application with the following modules:
- **classes** - Class management and organization
- **courses** - Course content and curriculum management
- **quizzes** - Quiz creation, delivery, and grading system
- **students** - Student enrollment and profile management
- **users** - User authentication and authorization
- **classpoint_backend** - Main project settings and configuration

### Frontend (`/Frontend/ClassPointAddIn`)
C# Office Add-in providing integration with Microsoft Office applications:
- Seamless classroom management from within Office
- Instructor dashboard
- Student interaction tools

### Student Portal (`/student-portal`)
React + Vite application for students:
- Quiz participation interface
- Course materials access
- Submission management
- Real-time progress tracking

## 🚀 Getting Started

### Prerequisites
- Python 3.8+
- Node.js 16+
- .NET Framework (for Office Add-in)
- SQLite (included) or PostgreSQL

### Backend Setup
1. Navigate to the Backend directory:
   ```bash
   cd Backend
   ```

2. Create a virtual environment:
   ```bash
   python -m venv .venv
   .venv\Scripts\activate
   ```

3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

4. Run migrations:
   ```bash
   python manage.py migrate
   ```

5. Create a superuser:
   ```bash
   python manage.py createsuperuser
   ```

6. Start the development server:
   ```bash
   python manage.py runserver
   ```

### Student Portal Setup
1. Navigate to the student portal:
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

### Frontend (Office Add-in) Setup
1. Open `Frontend/ClassPointAddIn/ClassPointAddIn.sln` in Visual Studio
2. Build the solution
3. Deploy to Microsoft Office following the Office Add-in deployment guide

## 📚 API Documentation

API documentation is available in the Postman collection located at:
```
Backend/docs/ClassPoint.postman_collection.json
```

Import this collection into Postman to explore all available endpoints.

## 🗄️ Database

The project uses SQLite by default (`db.sqlite3`). Database schema is managed through Django migrations in each app's `migrations/` directory.

## 📝 Additional Documentation

- [Setup Guide](SETUP_GUIDE.md) - Detailed installation instructions
- [Student Portal Docs](STUDENT_PORTAL_DOCS.md) - Student portal features
- [Database Setup](Backend/DATABASE_SETUP.md) - Database configuration
- [Quiz Deletion Guide](QUIZ_DELETION.md) - Quiz management
- [Submission Fix](SUBMISSION_FIX.md) - Submission troubleshooting

## 🐳 Docker

Run the application using Docker Compose:
```bash
docker-compose up --build
```

## 📦 Dependencies

### Backend
- Django
- Django REST Framework
- Django CORS Headers
- SQLite/PostgreSQL

### Frontend (Student Portal)
- React 18+
- TypeScript
- Vite
- Axios (for API calls)

## 🤝 Contributing

Contributions are welcome! Please follow the existing code structure and conventions.

## 📄 License

[Add your license information here]

## 📞 Support

For issues or questions, please refer to the relevant documentation files or contact the development team.
