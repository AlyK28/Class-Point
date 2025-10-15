from rest_framework import viewsets, permissions, status
from rest_framework.response import Response
from rest_framework.views import APIView
from django.shortcuts import get_object_or_404
from classes.models import Class
from .models import Student, StudentClassEnrollment
from .serializers import (
    StudentSerializer, StudentClassEnrollmentSerializer,
    StudentQuizSubmissionSerializer, StudentAnswerSerializer
)


class StudentViewSet(viewsets.ModelViewSet):
    queryset = Student.objects.all()
    serializer_class = StudentSerializer
    permission_classes = [permissions.AllowAny]


class StudentClassEnrollmentViewSet(viewsets.ModelViewSet):
    queryset = StudentClassEnrollment.objects.all()
    serializer_class = StudentClassEnrollmentSerializer
    permission_classes = [permissions.AllowAny]


class StudentQuizSubmissionViewSet(viewsets.ModelViewSet):
    queryset = StudentClassEnrollment.objects.all()
    serializer_class = StudentQuizSubmissionSerializer
    permission_classes = [permissions.AllowAny]


class StudentAnswerViewSet(viewsets.ModelViewSet):
    queryset = StudentClassEnrollment.objects.all()
    serializer_class = StudentAnswerSerializer
    permission_classes = [permissions.AllowAny]


class JoinClassView(APIView):
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        full_name = request.data.get("full_name")
        class_code = request.data.get("class_code")

        # Validate input
        if not full_name or not class_code:
            return Response(
                {"error": "Both full_name and class_code are required."},
                status=status.HTTP_400_BAD_REQUEST,
            )

        # Check if class exists and is active
        classroom = Class.objects.filter(code=class_code, active=True).first()
        if not classroom:
            return Response(
                {"error": "Invalid or inactive class code."},
                status=status.HTTP_404_NOT_FOUND,
            )

        # Create or get the student
        student, _ = Student.objects.get_or_create(full_name=full_name)

        # Check if already enrolled
        already_enrolled = StudentClassEnrollment.objects.filter(
            student=student, classroom=classroom
        ).exists()
        if already_enrolled:
            return Response(
                {"message": "You are already enrolled in this class."},
                status=status.HTTP_200_OK,
            )

        # Enroll the student
        enrollment = StudentClassEnrollment.objects.create(
            student=student, classroom=classroom
        )

        return Response(
            {
                "message": f"Successfully joined class {classroom.course.name if classroom.course else classroom.name} ({classroom.code})",
                "student_id": student.id,
                "class_id": classroom.id,
            },
            status=status.HTTP_201_CREATED,
        )
