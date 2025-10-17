from django.db import models, transaction, IntegrityError
from django.contrib.auth.models import User
from django.core.exceptions import ValidationError
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

    class Meta:
        constraints = [
            models.UniqueConstraint(
                fields=['teacher', 'course'],
                condition=models.Q(active=True),
                name='unique_active_class_per_teacher_course'
            )
        ]

    def clean(self):
        """Validate that teacher can't have more than one active class per course."""
        super().clean()
        
        if self.active:
            # Check if there's already an active class for this teacher and course
            existing_active = Class.objects.filter(
                teacher=self.teacher,
                course=self.course,
                active=True
            ).exclude(pk=self.pk)  # Exclude current instance if updating
            
            if existing_active.exists():
                raise ValidationError(
                    f"Teacher {self.teacher.username} already has an active class for course {self.course.name}. "
                    "Only one active class per course is allowed per teacher."
                )

    def save(self, *args, **kwargs):
        # Validate the model before saving
        self.clean()
        
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
