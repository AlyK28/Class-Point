from rest_framework import serializers
from .models import Student, StudentClassEnrollment, StudentQuizSubmission, StudentAnswer
from classes.models import Class
from quizzes.models import Quiz


def validate_no_duplicate_answer(quiz, request, serializer_instance):
    """Helper function to check if student has already answered a quiz."""
    if request and hasattr(request.user, 'student'):
        existing_answer = StudentAnswer.objects.filter(
            submission__student=request.user.student,
            submission__quiz=quiz
        ).exists()
        if existing_answer:
            raise serializers.ValidationError("You have already answered this quiz.")


class StudentSerializer(serializers.ModelSerializer):
    class Meta:
        model = Student
        fields = ['id', 'full_name', 'email', 'joined_at']


class ClassSimpleSerializer(serializers.ModelSerializer):
    """Lightweight serializer to represent a joined class."""
    course_name = serializers.CharField(source='course.name', read_only=True)
    teacher_name = serializers.CharField(source='teacher.username', read_only=True)

    class Meta:
        model = Class
        fields = ['id', 'code', 'course_name', 'teacher_name', 'active']  # ✅ removed 'name'


class StudentClassEnrollmentSerializer(serializers.ModelSerializer):
    # Return student as ID to match API tests expectations
    student = serializers.PrimaryKeyRelatedField(read_only=True)
    classroom = ClassSimpleSerializer(read_only=True)
    classroom_id = serializers.PrimaryKeyRelatedField(
        queryset=Class.objects.all(), write_only=True, source='classroom'
    )

    class Meta:
        model = StudentClassEnrollment
        fields = ['id', 'student', 'classroom', 'classroom_id', 'joined_at']


class StudentQuizSubmissionSerializer(serializers.ModelSerializer):
    quiz_title = serializers.CharField(source='quiz.title', read_only=True)
    quiz_type = serializers.CharField(source='quiz.quiz_type', read_only=True)

    class Meta:
        model = StudentQuizSubmission
        fields = [
            'id',
            'student',
            'quiz',
            'quiz_title',
            'quiz_type',
            'score',
            'is_late',
            'submitted_at',
        ]


# Base serializer for common fields
class BaseStudentAnswerSerializer(serializers.ModelSerializer):
    quiz_id = serializers.IntegerField(write_only=True, required=False)
    quiz_id_read = serializers.IntegerField(source='submission.quiz.id', read_only=True)
    quiz_title = serializers.CharField(source='submission.quiz.title', read_only=True)
    quiz_type = serializers.CharField(source='submission.quiz.quiz_type', read_only=True)
    student_name = serializers.CharField(source='submission.student.full_name', read_only=True)

    class Meta:
        model = StudentAnswer
        fields = [
            'id',
            'quiz_id',
            'quiz_id_read',
            'quiz_title',
            'quiz_type',
            'student_name',
            'answer_data',
            'uploaded_file',
            'submitted_at',
        ]
        read_only_fields = ['submitted_at']


# Specific serializers for each quiz type
class ShortAnswerSerializer(BaseStudentAnswerSerializer):
    answer_text = serializers.CharField(write_only=True)
    
    class Meta(BaseStudentAnswerSerializer.Meta):
        fields = BaseStudentAnswerSerializer.Meta.fields + ['answer_text']
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        if quiz.quiz_type != 'short_answer':
            raise serializers.ValidationError("This serializer is for short answer questions only")
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        answer_text = data.get('answer_text', '').strip()
        if not answer_text:
            raise serializers.ValidationError("Answer text is required")
        
        # Store in answer_data
        data['answer_data'] = {'answer_text': answer_text}
        return data
    
    def create(self, validated_data):
        # Remove quiz_id and answer_text from validated_data since they're not model fields
        validated_data.pop('quiz_id', None)
        validated_data.pop('answer_text', None)
        return super().create(validated_data)


class WordCloudAnswerSerializer(BaseStudentAnswerSerializer):
    answer_text = serializers.CharField(write_only=True)
    
    class Meta(BaseStudentAnswerSerializer.Meta):
        fields = BaseStudentAnswerSerializer.Meta.fields + ['answer_text']
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        if quiz.quiz_type != 'word_cloud':
            raise serializers.ValidationError("This serializer is for word cloud questions only")
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        answer_text = data.get('answer_text', '').strip()
        if not answer_text:
            raise serializers.ValidationError("Answer text is required")
        
        # Validate word cloud specific rules
        words = [word.strip() for word in answer_text.split(',') if word.strip()]
        
        # Check maximum words per student
        max_words = (quiz.properties or {}).get('max_words_per_student', 1)
        if len(words) > max_words:
            raise serializers.ValidationError(f"You can submit at most {max_words} word(s). You submitted {len(words)} words.")
        
        # Check minimum words if specified
        min_words = (quiz.properties or {}).get('min_words_per_student', 1)
        if len(words) < min_words:
            raise serializers.ValidationError(f"You must submit at least {min_words} word(s). You submitted {len(words)} words.")
        
        # Check for duplicate words if not allowed
        allow_duplicates = (quiz.properties or {}).get('allow_duplicates', False)
        if not allow_duplicates and len(words) != len(set(words)):
            raise serializers.ValidationError("Duplicate words are not allowed. Please remove duplicates.")
        
        # Check word length limits
        max_word_length = (quiz.properties or {}).get('max_word_length', 50)
        for word in words:
            if len(word) > max_word_length:
                raise serializers.ValidationError(f"Word '{word}' is too long. Maximum length is {max_word_length} characters.")
        
        # Check minimum word length
        min_word_length = (quiz.properties or {}).get('min_word_length', 1)
        for word in words:
            if len(word) < min_word_length:
                raise serializers.ValidationError(f"Word '{word}' is too short. Minimum length is {min_word_length} characters.")
        
        # Store in answer_data
        data['answer_data'] = {
            'answer_text': answer_text,
            'words': words,
            'word_count': len(words)
        }
        return data
    
    def create(self, validated_data):
        # Remove quiz_id and answer_text from validated_data since they're not model fields
        validated_data.pop('quiz_id', None)
        validated_data.pop('answer_text', None)
        return super().create(validated_data)


class MultipleChoiceAnswerSerializer(BaseStudentAnswerSerializer):
    selected_choice_indices = serializers.ListField(
        child=serializers.IntegerField(),
        write_only=True
    )
    
    class Meta(BaseStudentAnswerSerializer.Meta):
        fields = BaseStudentAnswerSerializer.Meta.fields + ['selected_choice_indices']
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        if quiz.quiz_type != 'multiple_choice':
            raise serializers.ValidationError("This serializer is for multiple choice questions only")
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        selected_indices = data.get('selected_choice_indices', [])
        if not selected_indices:
            raise serializers.ValidationError("Selected choice indices are required")
        
        # Validate choice indices
        choices = (quiz.properties or {}).get('choices', [])
        if not choices:
            raise serializers.ValidationError("Quiz has no choices defined")
        
        max_index = len(choices) - 1
        for index in selected_indices:
            if not isinstance(index, int) or index < 0 or index > max_index:
                raise serializers.ValidationError(f"Invalid choice index: {index}. Valid range: 0-{max_index}")
        
        # Validate choice limit
        max_choices = (quiz.properties or {}).get('max_choices', 1)
        if len(selected_indices) > max_choices:
            raise serializers.ValidationError(f"You can select at most {max_choices} choice(s). You selected {len(selected_indices)}.")
        
        # Validate minimum choices if specified
        min_choices = (quiz.properties or {}).get('min_choices', 1)
        if len(selected_indices) < min_choices:
            raise serializers.ValidationError(f"You must select at least {min_choices} choice(s). You selected {len(selected_indices)}.")
        
        # Check for duplicate indices
        if len(selected_indices) != len(set(selected_indices)):
            raise serializers.ValidationError("You cannot select the same choice multiple times.")
        
        # Store in answer_data
        data['answer_data'] = {'selected_choice_indices': selected_indices}
        return data


class DrawingAnswerSerializer(BaseStudentAnswerSerializer):
    # For drawing, we'll use uploaded_file for the image
    # Additional drawing metadata can go in answer_data
    
    class Meta(BaseStudentAnswerSerializer.Meta):
        pass
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        if quiz.quiz_type != 'drawing':
            raise serializers.ValidationError("This serializer is for drawing questions only")
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        if not data.get('uploaded_file'):
            raise serializers.ValidationError("Uploaded file is required for drawing questions")
        
        # Store any additional metadata in answer_data
        data['answer_data'] = data.get('answer_data', {})
        return data
    
    def create(self, validated_data):
        # Remove quiz_id from validated_data since it's not a model field
        validated_data.pop('quiz_id', None)
        return super().create(validated_data)


class ImageUploadAnswerSerializer(BaseStudentAnswerSerializer):
    # For image upload, we'll use uploaded_file for the image
    
    class Meta(BaseStudentAnswerSerializer.Meta):
        pass
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        if quiz.quiz_type != 'image_upload':
            raise serializers.ValidationError("This serializer is for image upload questions only")
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        if not data.get('uploaded_file'):
            raise serializers.ValidationError("Uploaded file is required for image upload questions")
        
        # Store any additional metadata in answer_data
        data['answer_data'] = data.get('answer_data', {})
        return data
    
    def create(self, validated_data):
        # Remove quiz_id from validated_data since it's not a model field
        validated_data.pop('quiz_id', None)
        return super().create(validated_data)


# Default serializer (for backward compatibility and general use)
class StudentAnswerSerializer(BaseStudentAnswerSerializer):
    """General purpose serializer that can handle any quiz type."""
    
    def validate(self, data):
        quiz_id = data.get('quiz_id')
        if not quiz_id:
            return data
        
        try:
            quiz = Quiz.objects.get(id=quiz_id)
        except Quiz.DoesNotExist:
            raise serializers.ValidationError("Invalid quiz ID")
        
        quiz_type = quiz.quiz_type
        
        # Check if student has already answered this quiz
        request = self.context.get('request')
        validate_no_duplicate_answer(quiz, request, self)
        
        # Validate based on quiz type
        if quiz_type == 'multiple_choice':
            selected_indices = data.get('answer_data', {}).get('selected_choice_indices', [])
            if not selected_indices:
                raise serializers.ValidationError("Multiple choice questions require selected_choice_indices in answer_data")
            
            # Validate choice indices
            choices = (quiz.properties or {}).get('choices', [])
            if not choices:
                raise serializers.ValidationError("Quiz has no choices defined")
            
            max_index = len(choices) - 1
            for index in selected_indices:
                if not isinstance(index, int) or index < 0 or index > max_index:
                    raise serializers.ValidationError(f"Invalid choice index: {index}. Valid range: 0-{max_index}")
            
            # Validate choice limit
            max_choices = (quiz.properties or {}).get('max_choices', 1)
            if len(selected_indices) > max_choices:
                raise serializers.ValidationError(f"You can select at most {max_choices} choice(s). You selected {len(selected_indices)}.")
            
            # Validate minimum choices if specified
            min_choices = (quiz.properties or {}).get('min_choices', 1)
            if len(selected_indices) < min_choices:
                raise serializers.ValidationError(f"You must select at least {min_choices} choice(s). You selected {len(selected_indices)}.")
            
            # Check for duplicate indices
            if len(selected_indices) != len(set(selected_indices)):
                raise serializers.ValidationError("You cannot select the same choice multiple times.")
        
        elif quiz_type == 'short_answer':
            answer_text = data.get('answer_data', {}).get('answer_text', '')
            if not answer_text or not str(answer_text).strip():
                raise serializers.ValidationError("Short answer questions require answer_text in answer_data")
        
        elif quiz_type == 'word_cloud':
            answer_text = data.get('answer_data', {}).get('answer_text', '')
            if not answer_text or not str(answer_text).strip():
                raise serializers.ValidationError("Word cloud questions require answer_text in answer_data")
            
            # Validate word cloud specific rules
            words = [word.strip() for word in answer_text.split(',') if word.strip()]
            
            # Check maximum words per student
            max_words = (quiz.properties or {}).get('max_words_per_student', 1)
            if len(words) > max_words:
                raise serializers.ValidationError(f"You can submit at most {max_words} word(s). You submitted {len(words)} words.")
            
            # Check minimum words if specified
            min_words = (quiz.properties or {}).get('min_words_per_student', 1)
            if len(words) < min_words:
                raise serializers.ValidationError(f"You must submit at least {min_words} word(s). You submitted {len(words)} words.")
            
            # Check for duplicate words if not allowed
            allow_duplicates = (quiz.properties or {}).get('allow_duplicates', False)
            if not allow_duplicates and len(words) != len(set(words)):
                raise serializers.ValidationError("Duplicate words are not allowed. Please remove duplicates.")
            
            # Check word length limits
            max_word_length = (quiz.properties or {}).get('max_word_length', 50)
            for word in words:
                if len(word) > max_word_length:
                    raise serializers.ValidationError(f"Word '{word}' is too long. Maximum length is {max_word_length} characters.")
            
            # Check minimum word length
            min_word_length = (quiz.properties or {}).get('min_word_length', 1)
            for word in words:
                if len(word) < min_word_length:
                    raise serializers.ValidationError(f"Word '{word}' is too short. Minimum length is {min_word_length} characters.")
        
        elif quiz_type in ['drawing', 'image_upload']:
            if not data.get('uploaded_file'):
                raise serializers.ValidationError(f"{quiz_type} questions require uploaded_file")
        
        return data
