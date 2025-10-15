from django.core.management.base import BaseCommand
from quizzes.models import QuizType

class Command(BaseCommand):
    help = "Seed default quiz types"

    def handle(self, *args, **options):
        default_types = [
            {"code": "multiple_choice", "name": "Multiple Choice", "has_options": True},
            {"code": "word_cloud", "name": "Word Cloud"},
            {"code": "short_answer", "name": "Short Answer"},
            {"code": "drawing", "name": "Drawing"},
            {"code": "image_upload", "name": "Image Upload"},
        ]

        for t in default_types:
            obj, created = QuizType.objects.get_or_create(code=t["code"], defaults=t)
            if created:
                self.stdout.write(self.style.SUCCESS(f"Created quiz type: {obj.name}"))
            else:
                self.stdout.write(f"Quiz type already exists: {obj.name}")
