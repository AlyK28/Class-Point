import { Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import LoginPage from './pages/LoginPage';
import QuizDashboard from './pages/QuizDashboard';

function App() {
  const [token, setToken] = useState<string | null>(null);
  const [studentInfo, setStudentInfo] = useState<any>(null);

  useEffect(() => {
    // Check for saved token in localStorage
    const savedToken = localStorage.getItem('studentToken');
    const savedStudentInfo = localStorage.getItem('studentInfo');
    
    if (savedToken && savedStudentInfo) {
      setToken(savedToken);
      setStudentInfo(JSON.parse(savedStudentInfo));
    }
  }, []);

  const handleLogin = (authData: any) => {
    setToken(authData.access_token);
    setStudentInfo({
      student_id: authData.student_id,
      class_id: authData.class_id,
      enrollment_id: authData.enrollment_id,
    });
    
    // Save to localStorage
    localStorage.setItem('studentToken', authData.access_token);
    localStorage.setItem('studentInfo', JSON.stringify({
      student_id: authData.student_id,
      class_id: authData.class_id,
      enrollment_id: authData.enrollment_id,
    }));
  };

  const handleLogout = () => {
    setToken(null);
    setStudentInfo(null);
    localStorage.removeItem('studentToken');
    localStorage.removeItem('studentInfo');
  };

  return (
    <div className="app">
      <Routes>
        <Route 
          path="/login" 
          element={
            token ? <Navigate to="/dashboard" replace /> : <LoginPage onLogin={handleLogin} />
          } 
        />
        <Route 
          path="/dashboard" 
          element={
            token ? (
              <QuizDashboard token={token} studentInfo={studentInfo} onLogout={handleLogout} />
            ) : (
              <Navigate to="/login" replace />
            )
          } 
        />
        <Route path="/" element={<Navigate to="/login" replace />} />
      </Routes>
    </div>
  );
}

export default App;
