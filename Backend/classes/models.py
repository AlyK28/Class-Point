from django.db import models, transaction, IntegrityError
from django.contrib.auth.models import User
from courses.models import Course
import random


class Class(models.Model):
    """
    Represents a live session created automatically when a slideshow starts.
    Each Class belongs to a Course and a Teacher.
    """
    code = models.CharField(max_length=4, unique=True, editable=False)
    teacher = models.ForeignKey(User, on_delete=models.CASCADE, related_name='created_classes')
    course = models.ForeignKey(Course, on_delete=models.CASCADE, related_name='classes')
    active = models.BooleanField(default=True)
    created_at = models.DateTimeField(auto_now_add=True)

    def save(self, *args, **kwargs):
        if not self.code:
            self.code = self.generate_unique_code()

        for attempt in range(5):
            try:
                with transaction.atomic():
                    super().save(*args, **kwargs)
                break
            except IntegrityError:
                self.code = self.generate_unique_code()
        else:
            raise IntegrityError("Failed to generate a unique code after several attempts")

    def generate_unique_code(self):
        """Generate a unique 4-digit join code."""
        return f"{random.randint(1000, 9999)}"

    def __str__(self):
        status = "Active" if self.active else "Inactive"
        return f"{self.course.name} ({self.code}) - {status}"
