import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import '../styles/LoginPage.css';

interface LoginPageProps {
  onLogin: (authData: any) => void;
}

function LoginPage({ onLogin }: LoginPageProps) {
  const [fullName, setFullName] = useState('');
  const [classCode, setClassCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const data = await api.joinClass(fullName, classCode);
      onLogin(data);
      navigate('/dashboard');
    } catch (err: any) {
      setError(err.message || 'Failed to join class. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <h1>🎓 ClassPoint Student</h1>
          <p>Join your class to participate in quizzes</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="fullName">Your Name</label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              placeholder="Enter your full name"
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="classCode">Class Code</label>
            <input
              id="classCode"
              type="text"
              value={classCode}
              onChange={(e) => setClassCode(e.target.value.toUpperCase())}
              placeholder="Enter 4-digit class code"
              maxLength={4}
              required
              disabled={loading}
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <button type="submit" disabled={loading} className="login-button">
            {loading ? 'Joining...' : 'Join Class'}
          </button>
        </form>

        <div className="login-footer">
          <p>Get the class code from your teacher to join</p>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
