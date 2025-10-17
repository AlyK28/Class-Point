"""
Helper functions and utilities for quiz operations.
Provides clean separation of concerns and prevents code duplication.
"""
from typing import Dict, List, Any, Optional
from django.core.exceptions import ValidationError
from django.db import models
import json


class QuizTypeValidator:
    """Validates quiz type specific requirements and constraints."""
    
    @staticmethod
    def validate_multiple_choice_options(choices: List[Dict[str, Any]]) -> None:
        """Validate multiple choice options structure and content."""
        if not choices:
            raise ValidationError("Multiple choice questions must have at least one option.")
        
        if len(choices) < 2:
            raise ValidationError("Multiple choice questions must have at least 2 options.")
        
        if len(choices) > 10:
            raise ValidationError("Multiple choice questions cannot have more than 10 options.")
        
        correct_answers = [choice for choice in choices if choice.get('is_correct', False)]
        if not correct_answers:
            raise ValidationError("At least one option must be marked as correct.")
        
        # Validate choice text
        for i, choice in enumerate(choices):
            if not choice.get('text', '').strip():
                raise ValidationError(f"Choice {i+1} cannot be empty.")
            
            if len(choice.get('text', '')) > 500:
                raise ValidationError(f"Choice {i+1} text cannot exceed 500 characters.")
    
    @staticmethod
    def validate_short_answer_content(question_text: str, correct_answer: str = None, 
                                    expected_keywords: str = None) -> None:
        """Validate short answer question content."""
        if not question_text.strip():
            raise ValidationError("Question text cannot be empty.")
        
        if len(question_text) > 2000:
            raise ValidationError("Question text cannot exceed 2000 characters.")
        
        if correct_answer and len(correct_answer) > 500:
            raise ValidationError("Correct answer cannot exceed 500 characters.")
        
        if expected_keywords and len(expected_keywords) > 1000:
            raise ValidationError("Expected keywords cannot exceed 1000 characters.")


class QuizContentManager:
    """Utility helpers for MCQ choice processing (kept for compatibility)."""
    @staticmethod
    def get_correct_choices(choices: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        return [choice for choice in choices if choice.get('is_correct', False)]
    
    @staticmethod
    def format_choices_for_display(choices: List[Dict[str, Any]], randomize: bool = False) -> List[Dict[str, Any]]:
        import random
        formatted_choices = [
            {
                'id': i,
                'text': choice.get('text', ''),
                'is_correct': choice.get('is_correct', False)
            }
            for i, choice in enumerate(choices)
        ]
        if randomize:
            random.shuffle(formatted_choices)
            for i, choice in enumerate(formatted_choices):
                choice['id'] = i
        return formatted_choices


class QuizGradingHelper:
    """Handles quiz grading and scoring logic."""
    
    @staticmethod
    def grade_multiple_choice(student_selections: List[int], 
                            correct_choices: List[Dict[str, Any]], 
                            allow_multiple: bool = False) -> Dict[str, Any]:
        """Grade multiple choice answers."""
        correct_indices = [i for i, choice in enumerate(correct_choices) if choice.get('is_correct', False)]
        
        if not allow_multiple:
            # Single choice - exact match required
            is_correct = len(student_selections) == 1 and student_selections[0] in correct_indices
            score = 1 if is_correct else 0
            correct_selections = [sel for sel in student_selections if sel in correct_indices]
            incorrect_selections = [sel for sel in student_selections if sel not in correct_indices]
        else:
            # Multiple choice - partial credit possible
            correct_selections = [sel for sel in student_selections if sel in correct_indices]
            incorrect_selections = [sel for sel in student_selections if sel not in correct_indices]
            
            # Calculate score based on correct vs incorrect selections
            score = max(0, len(correct_selections) - len(incorrect_selections))
            is_correct = score > 0
        
        return {
            'is_correct': is_correct,
            'score': score,
            'correct_selections': correct_selections,
            'incorrect_selections': incorrect_selections,
            'expected_correct': correct_indices
        }
    
    @staticmethod
    def grade_short_answer(student_answer: str, correct_answer: str = None, 
                          expected_keywords: str = None, case_sensitive: bool = False) -> Dict[str, Any]:
        """Grade short answer questions."""
        if not student_answer.strip():
            return {'is_correct': False, 'score': 0, 'feedback': 'No answer provided'}
        
        student_text = student_answer if case_sensitive else student_answer.lower()
        
        # Check exact match first
        if correct_answer:
            correct_text = correct_answer if case_sensitive else correct_answer.lower()
            if student_text == correct_text:
                return {'is_correct': True, 'score': 1, 'feedback': 'Correct!'}
        
        # Check keyword matching
        if expected_keywords:
            keywords = [kw.strip().lower() for kw in expected_keywords.split(',') if kw.strip()]
            if not case_sensitive:
                student_text = student_text.lower()
            
            matched_keywords = [kw for kw in keywords if kw in student_text]
            if matched_keywords:
                score = len(matched_keywords) / len(keywords)
                return {
                    'is_correct': score >= 0.5,  # 50% keyword match threshold
                    'score': score,
                    'feedback': f'Matched keywords: {", ".join(matched_keywords)}'
                }
        
        return {'is_correct': False, 'score': 0, 'feedback': 'Answer does not match expected content'}


class QuizStatisticsHelper:
    """Calculates quiz statistics and analytics."""
    
    @staticmethod
    def calculate_quiz_statistics(submissions: models.QuerySet) -> Dict[str, Any]:
        """Calculate comprehensive quiz statistics."""
        total_submissions = submissions.count()
        if total_submissions == 0:
            return {
                'total_submissions': 0,
                'average_score': 0,
                'completion_rate': 0,
                'score_distribution': {}
            }
        
        scores = [sub.score for sub in submissions if sub.score is not None]
        average_score = sum(scores) / len(scores) if scores else 0
        
        # Score distribution
        score_ranges = {
            '0-20%': 0,
            '21-40%': 0,
            '41-60%': 0,
            '61-80%': 0,
            '81-100%': 0
        }
        
        for score in scores:
            if score <= 0.2:
                score_ranges['0-20%'] += 1
            elif score <= 0.4:
                score_ranges['21-40%'] += 1
            elif score <= 0.6:
                score_ranges['41-60%'] += 1
            elif score <= 0.8:
                score_ranges['61-80%'] += 1
            else:
                score_ranges['81-100%'] += 1
        
        return {
            'total_submissions': total_submissions,
            'average_score': round(average_score, 2),
            'completion_rate': len(scores) / total_submissions,
            'score_distribution': score_ranges,
            'highest_score': max(scores) if scores else 0,
            'lowest_score': min(scores) if scores else 0
        }
