from django.urls import path
from .views import ClassCreateView

urlpatterns = [
    path('create/', ClassCreateView.as_view(), name='create_class'),
]
