import { useState } from 'react';
import '../../styles/QuizTypes.css';

interface ImageUploadQuizProps {
  quiz: any;
  onSubmit: (answerData: any, file?: File) => void;
  submitting: boolean;
}

function ImageUploadQuiz({ quiz, onSubmit, submitting }: ImageUploadQuizProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const properties = quiz.properties || {};
  const maxFileSize = (properties.max_file_size_mb || 5) * 1024 * 1024; // Convert to bytes
  const allowedFormats = (properties.allowed_formats || 'jpg,png,jpeg').split(',');

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Check file size
    if (file.size > maxFileSize) {
      alert(`File size must be less than ${properties.max_file_size_mb || 5}MB`);
      return;
    }

    // Check file type
    const fileExtension = file.name.split('.').pop()?.toLowerCase();
    if (!allowedFormats.includes(fileExtension || '')) {
      alert(`Allowed formats: ${allowedFormats.join(', ')}`);
      return;
    }

    setSelectedFile(file);

    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleSubmit = () => {
    if (!selectedFile) {
      alert('Please select an image');
      return;
    }

    onSubmit({ image_type: 'upload' }, selectedFile);
  };

  return (
    <div className="quiz-type-container">
      <div className="question-text">{properties.question_text}</div>
      
      <div className="image-upload-container">
        <input
          type="file"
          accept={allowedFormats.map(f => `.${f}`).join(',')}
          onChange={handleFileChange}
          disabled={submitting}
          className="file-input"
          id="imageUpload"
        />
        <label htmlFor="imageUpload" className="file-input-label">
          {selectedFile ? selectedFile.name : 'Choose Image'}
        </label>

        {preview && (
          <div className="image-preview">
            <img src={preview} alt="Preview" />
          </div>
        )}

        <p className="hint-text">
          Max size: {properties.max_file_size_mb || 5}MB | 
          Formats: {allowedFormats.join(', ')}
        </p>
      </div>

      <button
        className="submit-button"
        onClick={handleSubmit}
        disabled={submitting || !selectedFile}
      >
        {submitting ? 'Uploading...' : 'Submit Image'}
      </button>
    </div>
  );
}

export default ImageUploadQuiz;
