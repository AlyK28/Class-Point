import '../styles/QuizCard.css';

interface QuizCardProps {
  quiz: any;
  onClick: () => void;
}

function QuizCard({ quiz, onClick }: QuizCardProps) {
  const getQuizTypeIcon = (type: string) => {
    switch (type) {
      case 'multiple_choice': return '📝';
      case 'short_answer': return '✍️';
      case 'word_cloud': return '☁️';
      case 'drawing': return '🎨';
      case 'image_upload': return '📷';
      default: return '❓';
    }
  };

  const getQuizTypeName = (type: string) => {
    switch (type) {
      case 'multiple_choice': return 'Multiple Choice';
      case 'short_answer': return 'Short Answer';
      case 'word_cloud': return 'Word Cloud';
      case 'drawing': return 'Drawing';
      case 'image_upload': return 'Image Upload';
      default: return type;
    }
  };

  return (
    <div className="quiz-card" onClick={onClick}>
      <div className="quiz-icon">{getQuizTypeIcon(quiz.quiz_type)}</div>
      <div className="quiz-content">
        <h3>{quiz.title}</h3>
        <p className="quiz-type">{getQuizTypeName(quiz.quiz_type)}</p>
        {quiz.show_timer && quiz.auto_close_after_seconds && (
          <p className="quiz-timer">⏱️ {quiz.auto_close_after_seconds}s</p>
        )}
      </div>
      <div className="quiz-action">
        <span className="participate-text">Click to participate →</span>
      </div>
    </div>
  );
}

export default QuizCard;
