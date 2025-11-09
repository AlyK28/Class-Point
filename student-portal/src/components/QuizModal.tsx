import { useState } from 'react';
import api from '../services/api';
import MultipleChoiceQuiz from './quiz-types/MultipleChoiceQuiz';
import ShortAnswerQuiz from './quiz-types/ShortAnswerQuiz';
import WordCloudQuiz from './quiz-types/WordCloudQuiz';
import DrawingQuiz from './quiz-types/DrawingQuiz';
import ImageUploadQuiz from './quiz-types/ImageUploadQuiz';
import '../styles/QuizModal.css';

interface QuizModalProps {
  quiz: any;
  token: string;
  onClose: () => void;
}

function QuizModal({ quiz, token, onClose }: QuizModalProps) {
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (answerData: any, file?: File) => {
    setSubmitting(true);
    setError('');

    try {
      await api.submitAnswer(token, quiz.id, answerData, file);
      setSubmitted(true);
      setTimeout(() => {
        onClose();
      }, 2000);
    } catch (err: any) {
      setError(err.message || 'Failed to submit answer');
    } finally {
      setSubmitting(false);
    }
  };

  const renderQuizContent = () => {
    if (submitted) {
      return (
        <div className="submission-success">
          <div className="success-icon">✅</div>
          <h3>Answer Submitted!</h3>
          <p>Your response has been recorded successfully.</p>
        </div>
      );
    }

    const props = {
      quiz,
      onSubmit: handleSubmit,
      submitting,
    };

    switch (quiz.quiz_type) {
      case 'multiple_choice':
        return <MultipleChoiceQuiz {...props} />;
      case 'short_answer':
        return <ShortAnswerQuiz {...props} />;
      case 'word_cloud':
        return <WordCloudQuiz {...props} />;
      case 'drawing':
        return <DrawingQuiz {...props} />;
      case 'image_upload':
        return <ImageUploadQuiz {...props} />;
      default:
        return <div>Unsupported quiz type: {quiz.quiz_type}</div>;
    }
  };

  return (
    <div className="quiz-modal-overlay" onClick={onClose}>
      <div className="quiz-modal" onClick={(e) => e.stopPropagation()}>
        <div className="quiz-modal-header">
          <h2>{quiz.title}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        <div className="quiz-modal-body">
          {error && <div className="error-message">{error}</div>}
          {renderQuizContent()}
        </div>
      </div>
    </div>
  );
}

export default QuizModal;
