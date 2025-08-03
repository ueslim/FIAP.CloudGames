import React, { useState, useEffect } from 'react';
import Login from './components/Login';
import Register from './components/Register';
import { authService } from './services/api';
import './styles/global.css';

const App: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    // Verificar se o usuário já está autenticado
    if (authService.isAuthenticated()) {
      const userData = authService.getUser();
      setUser(userData);
      setIsAuthenticated(true);
    }
  }, []);

  const handleLoginSuccess = () => {
    const userData = authService.getUser();
    setUser(userData);
    setIsAuthenticated(true);
  };

  const handleRegisterSuccess = () => {
    const userData = authService.getUser();
    setUser(userData);
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setUser(null);
    setIsLogin(true);
  };

  const switchToRegister = () => {
    setIsLogin(false);
  };

  const switchToLogin = () => {
    setIsLogin(true);
  };

  // Se o usuário está autenticado, mostrar dashboard simples
  if (isAuthenticated && user) {
    return (
      <div className="container">
        <h1 className="form-title">Bem-vindo!</h1>
        <div style={{ textAlign: 'center', marginBottom: '30px' }}>
          <p style={{ fontSize: '18px', color: '#666', marginBottom: '20px' }}>
            Olá, <strong>{user.name}</strong>!
          </p>
          <p style={{ fontSize: '14px', color: '#888' }}>
            Email: {user.email}
          </p>
        </div>
        
        <button
          onClick={handleLogout}
          className="btn btn-secondary"
        >
          Sair
        </button>
      </div>
    );
  }

  // Mostrar formulário de login ou registro
  return (
    <>
      {isLogin ? (
        <Login
          onSwitchToRegister={switchToRegister}
          onLoginSuccess={handleLoginSuccess}
        />
      ) : (
        <Register
          onSwitchToLogin={switchToLogin}
          onRegisterSuccess={handleRegisterSuccess}
        />
      )}
    </>
  );
};

export default App; 