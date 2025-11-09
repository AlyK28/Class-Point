import { useState } from 'react';
import '../../styles/QuizTypes.css';

interface ShortAnswerQuizProps {
  quiz: any;
  onSubmit: (answerData: any) => void;
  submitting: boolean;
}

function ShortAnswerQuiz({ quiz, onSubmit, submitting }: ShortAnswerQuizProps) {
  const [answer, setAnswer] = useState('');
  const properties = quiz.properties || {};

  const handleSubmit = () => {
    if (!answer.trim()) {
      alert('Please enter an answer');
      return;
    }

    onSubmit({
      answer_text: answer,
    });
  };

  return (
    <div className="quiz-type-container">
      <div className="question-text">{properties.question_text}</div>
      
      <textarea
        className="short-answer-input"
        value={answer}
        onChange={(e) => setAnswer(e.target.value)}
        placeholder="Type your answer here..."
        rows={5}
        disabled={submitting}
      />

      <button
        className="submit-button"
        onClick={handleSubmit}
        disabled={submitting || !answer.trim()}
      >
        {submitting ? 'Submitting...' : 'Submit Answer'}
      </button>
    </div>
  );
}

export default ShortAnswerQuiz;
