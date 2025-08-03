import React, { useState } from 'react';
import { authService } from '../services/api';
import { LoginRequest } from '../types/auth';
import '../styles/global.css';

interface LoginProps {
  onSwitchToRegister: () => void;
  onLoginSuccess: () => void;
}

const Login: React.FC<LoginProps> = ({ onSwitchToRegister, onLoginSuccess }) => {
  const [formData, setFormData] = useState<LoginRequest>({
    email: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    // Limpar erros quando o usuário começa a digitar
    if (error) setError('');
  };

  const validateForm = (): boolean => {
    if (!formData.email.trim()) {
      setError('Email é obrigatório');
      return false;
    }
    if (!formData.password.trim()) {
      setError('Senha é obrigatória');
      return false;
    }
    if (formData.password.length < 6) {
      setError('Senha deve ter pelo menos 6 caracteres');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) return;

    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const response = await authService.login(formData);
      
      // Salvar token e dados do usuário
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      setSuccess('Login realizado com sucesso!');
      
      // Aguardar um pouco para mostrar a mensagem de sucesso
      setTimeout(() => {
        onLoginSuccess();
      }, 1000);
      
    } catch (err: any) {
      setError(err.message || 'Erro ao fazer login');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <h1 className="form-title">Login</h1>
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="email" className="form-label">
            Email
          </label>
          <input
            type="email"
            id="email"
            name="email"
            className={`form-input ${error && !formData.email ? 'error' : ''}`}
            value={formData.email}
            onChange={handleInputChange}
            placeholder="Digite seu email"
            disabled={loading}
          />
        </div>

        <div className="form-group">
          <label htmlFor="password" className="form-label">
            Senha
          </label>
          <input
            type="password"
            id="password"
            name="password"
            className={`form-input ${error && !formData.password ? 'error' : ''}`}
            value={formData.password}
            onChange={handleInputChange}
            placeholder="Digite sua senha"
            disabled={loading}
          />
        </div>

        {error && <span className="error-message">{error}</span>}
        {success && <span className="success-message">{success}</span>}

        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading}
        >
          {loading ? (
            <>
              <span className="loading"></span>
              Entrando...
            </>
          ) : (
            'Entrar'
          )}
        </button>
      </form>

      <div className="form-footer">
        <p>
          Não tem uma conta?{' '}
          <a href="#" onClick={onSwitchToRegister}>
            Registre-se aqui
          </a>
        </p>
      </div>
    </div>
  );
};

export default Login; 