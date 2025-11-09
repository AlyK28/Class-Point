import { useState } from 'react';
import '../../styles/QuizTypes.css';

interface MultipleChoiceQuizProps {
  quiz: any;
  onSubmit: (answerData: any) => void;
  submitting: boolean;
}

function MultipleChoiceQuiz({ quiz, onSubmit, submitting }: MultipleChoiceQuizProps) {
  const [selectedChoices, setSelectedChoices] = useState<number[]>([]);
  const properties = quiz.properties || {};
  const choices = properties.choices || [];
  const allowMultiple = properties.allow_multiple_choices || false;

  const handleChoiceClick = (index: number) => {
    if (allowMultiple) {
      setSelectedChoices(prev =>
        prev.includes(index)
          ? prev.filter(i => i !== index)
          : [...prev, index]
      );
    } else {
      setSelectedChoices([index]);
    }
  };

  const handleSubmit = () => {
    if (selectedChoices.length === 0) {
      alert('Please select at least one answer');
      return;
    }

    // Send selected_choices (indices) - the API will transform to selected_choice_indices
    onSubmit({
      selected_choices: selectedChoices,
    });
  };

  return (
    <div className="quiz-type-container">
      <div className="question-text">{properties.question_text}</div>
      
      <div className="choices-container">
        {choices.map((choice: any, index: number) => (
          <button
            key={index}
            className={`choice-button ${selectedChoices.includes(index) ? 'selected' : ''}`}
            onClick={() => handleChoiceClick(index)}
            disabled={submitting}
          >
            <span className="choice-letter">{String.fromCharCode(65 + index)}</span>
            <span className="choice-text">{choice.text}</span>
          </button>
        ))}
      </div>

      {allowMultiple && (
        <p className="hint-text">You can select multiple answers</p>
      )}

      <button
        className="submit-button"
        onClick={handleSubmit}
        disabled={submitting || selectedChoices.length === 0}
      >
        {submitting ? 'Submitting...' : 'Submit Answer'}
      </button>
    </div>
  );
}

export default MultipleChoiceQuiz;
