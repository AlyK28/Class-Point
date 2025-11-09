import { useState, useEffect } from 'react';
import api from '../services/api';
import QuizCard from '../components/QuizCard';
import QuizModal from '../components/QuizModal';
import '../styles/QuizDashboard.css';

interface QuizDashboardProps {
  token: string;
  studentInfo: any;
  onLogout: () => void;
}

function QuizDashboard({ token, studentInfo, onLogout }: QuizDashboardProps) {
  const [quizzes, setQuizzes] = useState<any[]>([]);
  const [classInfo, setClassInfo] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedQuiz, setSelectedQuiz] = useState<any>(null);
  const [refreshInterval, setRefreshInterval] = useState<NodeJS.Timeout | null>(null);

  const fetchQuizzes = async () => {
    try {
      const data = await api.getQuizzes(token);
      setQuizzes(data.quizzes || []);
      setClassInfo(data.class_info);
      setError('');
    } catch (err: any) {
      setError(err.message || 'Failed to load quizzes');
      // If unauthorized, logout
      if (err.message.includes('401') || err.message.includes('unauthorized')) {
        onLogout();
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchQuizzes();
    
    // Poll for new quizzes every 5 seconds
    const interval = setInterval(fetchQuizzes, 5000);
    setRefreshInterval(interval);

    return () => {
      if (interval) clearInterval(interval);
    };
  }, [token]);

  const handleQuizClick = (quiz: any) => {
    setSelectedQuiz(quiz);
  };

  const handleCloseModal = () => {
    setSelectedQuiz(null);
    fetchQuizzes(); // Refresh after submission
  };

  if (loading) {
    return (
      <div className="dashboard-container">
        <div className="loading">Loading quizzes...</div>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-content">
          <div>
            <h1>🎓 {classInfo?.course_name || 'ClassPoint'}</h1>
            <p className="class-code">Class Code: <strong>{classInfo?.code}</strong></p>
          </div>
          <button onClick={onLogout} className="logout-button">
            Logout
          </button>
        </div>
      </header>

      <main className="dashboard-main">
        {error && <div className="error-message">{error}</div>}

        {quizzes.length === 0 ? (
          <div className="empty-state">
            <h2>No Quizzes Available</h2>
            <p>Your teacher hasn't started any quizzes yet. Check back soon!</p>
            <div className="waiting-animation">⏳</div>
          </div>
        ) : (
          <>
            <div className="quizzes-header">
              <h2>Available Quizzes ({quizzes.length})</h2>
              <button onClick={fetchQuizzes} className="refresh-button">
                🔄 Refresh
              </button>
            </div>
            
            <div className="quizzes-grid">
              {quizzes.map((quiz) => (
                <QuizCard 
                  key={quiz.id} 
                  quiz={quiz} 
                  onClick={() => handleQuizClick(quiz)}
                />
              ))}
            </div>
          </>
        )}
      </main>

      {selectedQuiz && (
        <QuizModal 
          quiz={selectedQuiz} 
          token={token}
          onClose={handleCloseModal}
        />
      )}
    </div>
  );
}

export default QuizDashboard;
