from django.db import models
from django.contrib.auth.models import User


class Course(models.Model):
    """
    Represents a PowerPoint course (e.g., 'Math101.pptx').
    """
    name = models.CharField(max_length=255, unique=True)
    teacher = models.ForeignKey(User, on_delete=models.CASCADE, related_name='courses')
    created_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return self.name
