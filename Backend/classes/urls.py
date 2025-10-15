from django.urls import path
from .views import create_class_from_powerpoint

urlpatterns = [
    path('create-class/', create_class_from_powerpoint, name='create_class_from_ppt'),
]
