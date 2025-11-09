import { useState } from 'react';
import '../../styles/QuizTypes.css';

interface WordCloudQuizProps {
  quiz: any;
  onSubmit: (answerData: any) => void;
  submitting: boolean;
}

function WordCloudQuiz({ quiz, onSubmit, submitting }: WordCloudQuizProps) {
  const [words, setWords] = useState<string[]>([]);
  const [currentWord, setCurrentWord] = useState('');
  const properties = quiz.properties || {};
  const maxWords = properties.max_words_per_student || 3;

  const handleAddWord = () => {
    const trimmedWord = currentWord.trim();
    if (!trimmedWord) return;

    if (words.length >= maxWords) {
      alert(`You can only submit up to ${maxWords} word(s)`);
      return;
    }

    setWords([...words, trimmedWord]);
    setCurrentWord('');
  };

  const handleRemoveWord = (index: number) => {
    setWords(words.filter((_, i) => i !== index));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddWord();
    }
  };

  const handleSubmit = () => {
    if (words.length === 0) {
      alert('Please add at least one word');
      return;
    }

    // Backend expects answer_text as comma-separated string
    onSubmit({
      answer_text: words.join(', '),
    });
  };

  return (
    <div className="quiz-type-container">
      <div className="question-text">{properties.question_text}</div>
      
      <div className="word-cloud-input-container">
        <input
          type="text"
          className="word-cloud-input"
          value={currentWord}
          onChange={(e) => setCurrentWord(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Enter a word or phrase"
          disabled={submitting || words.length >= maxWords}
        />
        <button
          className="add-word-button"
          onClick={handleAddWord}
          disabled={submitting || !currentWord.trim() || words.length >= maxWords}
        >
          Add
        </button>
      </div>

      {words.length > 0 && (
        <div className="words-list">
          {words.map((word, index) => (
            <div key={index} className="word-tag">
              <span>{word}</span>
              <button
                className="remove-word"
                onClick={() => handleRemoveWord(index)}
                disabled={submitting}
              >
                ×
              </button>
            </div>
          ))}
        </div>
      )}

      <p className="hint-text">
        {words.length} / {maxWords} word{maxWords !== 1 ? 's' : ''}
      </p>

      <button
        className="submit-button"
        onClick={handleSubmit}
        disabled={submitting || words.length === 0}
      >
        {submitting ? 'Submitting...' : 'Submit Words'}
      </button>
    </div>
  );
}

export default WordCloudQuiz;
