from django.db import models, transaction, IntegrityError
from django.contrib.auth.models import User
import random

class Class(models.Model):
    """
    Represents a class created by a teacher that students can join via a 4-digit code.
    """
    name = models.CharField(max_length=100)
    subject = models.CharField(max_length=100, blank=True, null=True)
    code = models.CharField(max_length=4, unique=True, editable=False)
    teacher = models.ForeignKey(User, on_delete=models.CASCADE, related_name='created_classes')
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
        return f"{self.name} ({self.code}) - {status}"
