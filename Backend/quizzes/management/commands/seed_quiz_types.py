from django.core.management.base import BaseCommand


class Command(BaseCommand):
    help = "Deprecated: quiz types are now enum choices on Quiz. No seeding needed."

    def handle(self, *args, **options):
        self.stdout.write(self.style.WARNING("No action performed. QuizType model removed."))
