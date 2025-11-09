const API_BASE_URL = 'http://localhost:8000/api';

export const api = {
  // Student authentication
  joinClass: async (fullName: string, classCode: string) => {
    try {
      const response = await fetch(`${API_BASE_URL}/students/join/`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          full_name: fullName,
          class_code: classCode,
        }),
      });
      
      // Get the response text first
      const text = await response.text();
      console.log('Response status:', response.status);
      console.log('Response text:', text);
      
      if (!response.ok) {
        // Try to parse as JSON, fallback to text
        let errorMessage = 'Failed to join class';
        try {
          const error = JSON.parse(text);
          errorMessage = error.error || errorMessage;
        } catch {
          errorMessage = text || errorMessage;
        }
        throw new Error(errorMessage);
      }
      
      // Parse the successful response
      return JSON.parse(text);
    } catch (error) {
      console.error('Join class error:', error);
      throw error;
    }
  },

  // Get quizzes for authenticated student
  getQuizzes: async (token: string) => {
    try {
      console.log('Fetching quizzes with token:', token ? 'present' : 'missing');
      
      const response = await fetch(`${API_BASE_URL}/students/quizzes/`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
      
      console.log('Quiz fetch response status:', response.status);
      
      if (!response.ok) {
        const text = await response.text();
        console.error('Quiz fetch error:', text);
        throw new Error(`Failed to fetch quizzes (${response.status})`);
      }
      
      const data = await response.json();
      console.log('Quiz data received:', data);
      return data;
    } catch (error) {
      console.error('Get quizzes error:', error);
      throw error;
    }
  },

  // Submit quiz answer
  submitAnswer: async (token, quizId, answerData, uploadedFile = null) => {
    const formData = new FormData();
    formData.append('quiz_id', quizId.toString());
    
    // For multiple choice, send as array elements
    if (answerData.selected_choices !== undefined) {
      // Send each index as a separate form field for DRF ListField
      answerData.selected_choices.forEach((index: number) => {
        formData.append('selected_choice_indices', index.toString());
      });
    }
    
    // For short answer and word cloud
    if (answerData.answer_text !== undefined) {
      formData.append('answer_text', answerData.answer_text);
    }
    
    // For any other answer data, send as answer_data JSON
    if (Object.keys(answerData).length > 0 && !answerData.selected_choices && !answerData.answer_text) {
      formData.append('answer_data', JSON.stringify(answerData));
    }
    
    if (uploadedFile) {
      formData.append('uploaded_file', uploadedFile);
    }

    console.log('Submitting answer:', { quizId, answerData, hasFile: !!uploadedFile });

    const response = await fetch(`${API_BASE_URL}/students/answers/`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
      body: formData,
    });
    
    if (!response.ok) {
      const text = await response.text();
      console.error('Submit answer error:', text);
      try {
        const error = JSON.parse(text);
        throw new Error(error.error || JSON.stringify(error) || 'Failed to submit answer');
      } catch {
        throw new Error(text || 'Failed to submit answer');
      }
    }
    
    return response.json();
  },

  // Get student's submissions
  getSubmissions: async (token) => {
    const response = await fetch(`${API_BASE_URL}/students/submissions/`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to fetch submissions');
    }
    
    return response.json();
  },
};

export default api;
