"""
Quick test script to create sample data for testing the Student Portal.
Run this after starting the Django server to create a test class and quiz.
"""
import os
import django

# Setup Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'classpoint_backend.settings')
django.setup()

from django.contrib.auth.models import User
from courses.models import Course
from classes.models import Class
from quizzes.models import Quiz

def create_test_data():
    """Create a teacher, course, class, and sample quizzes for testing."""
    
    print("🚀 Creating test data for Student Portal...")
    
    # Create or get teacher
    teacher, created = User.objects.get_or_create(
        username='testteacher',
        defaults={
            'email': 'teacher@test.com',
            'is_staff': True,
        }
    )
    if created:
        teacher.set_password('password123')
        teacher.save()
        print(f"✓ Created teacher: {teacher.username}")
    else:
        print(f"✓ Using existing teacher: {teacher.username}")
    
    # Create or get course
    course, created = Course.objects.get_or_create(
        name='Test Course - Math 101',
        defaults={'teacher': teacher}
    )
    print(f"✓ {'Created' if created else 'Using existing'} course: {course.name}")
    
    # Create or get active class
    active_class = Class.objects.filter(
        course=course,
        teacher=teacher,
        active=True
    ).first()
    
    if not active_class:
        active_class = Class.objects.create(
            course=course,
            teacher=teacher,
            active=True
        )
        print(f"✓ Created class with code: {active_class.code}")
    else:
        print(f"✓ Using existing class with code: {active_class.code}")
    
    # Create sample quizzes
    quizzes_data = [
        {
            'title': 'What is 2 + 2?',
            'quiz_type': 'multiple_choice',
            'properties': {
                'question_text': 'What is 2 + 2?',
                'choices': [
                    {'text': '3', 'is_correct': False},
                    {'text': '4', 'is_correct': True},
                    {'text': '5', 'is_correct': False},
                    {'text': '6', 'is_correct': False}
                ],
                'allow_multiple_choices': False
            }
        },
        {
            'title': 'Explain Photosynthesis',
            'quiz_type': 'short_answer',
            'properties': {
                'question_text': 'In your own words, explain the process of photosynthesis.',
                'correct_answer': 'Process where plants convert light into energy',
                'expected_keywords': 'light,chlorophyll,oxygen,glucose,carbon dioxide'
            }
        },
        {
            'title': 'Words Related to Science',
            'quiz_type': 'word_cloud',
            'properties': {
                'question_text': 'Name up to 3 words related to science.',
                'max_words_per_student': 3,
                'allow_duplicates': True
            }
        },
        {
            'title': 'Draw a Circle',
            'quiz_type': 'drawing',
            'properties': {
                'question_text': 'Draw a perfect circle (or as close as you can!).',
                'canvas_width': 600,
                'canvas_height': 400
            }
        },
        {
            'title': 'Upload Your Notes',
            'quiz_type': 'image_upload',
            'properties': {
                'question_text': 'Upload a photo of your class notes.',
                'max_file_size_mb': 5,
                'allowed_formats': 'jpg,png,jpeg'
            }
        }
    ]
    
    print("\n📝 Creating sample quizzes...")
    for quiz_data in quizzes_data:
        quiz, created = Quiz.objects.get_or_create(
            title=quiz_data['title'],
            course=course,
            quiz_type=quiz_data['quiz_type'],
            defaults={
                'created_by': teacher,
                'properties': quiz_data['properties']
            }
        )
        if created:
            print(f"  ✓ Created: {quiz.title} ({quiz.quiz_type})")
        else:
            print(f"  ✓ Exists: {quiz.title} ({quiz.quiz_type})")
    
    print("\n" + "="*60)
    print("🎉 Test data created successfully!")
    print("="*60)
    print(f"\n📋 CLASS CODE: {active_class.code}")
    print(f"👨‍🏫 Teacher: {teacher.username}")
    print(f"📚 Course: {course.name}")
    print(f"📝 Quizzes created: {len(quizzes_data)}")
    print("\n" + "="*60)
    print("\n📖 Next Steps:")
    print("1. Open the student portal: http://localhost:5173")
    print(f"2. Enter any name and class code: {active_class.code}")
    print("3. You should see all 5 quizzes!")
    print("4. Try submitting answers for different quiz types")
    print("\n💡 To view submissions:")
    print("   - Django Admin: http://localhost:8000/admin")
    print(f"   - Username: {teacher.username}")
    print("   - Password: password123")
    print("\n" + "="*60)

if __name__ == '__main__':
    create_test_data()
