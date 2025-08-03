import React, { useState } from 'react';
import { authService } from '../services/api';
import { RegisterRequest } from '../types/auth';
import '../styles/global.css';

interface RegisterProps {
  onSwitchToLogin: () => void;
  onRegisterSuccess: () => void;
}

const Register: React.FC<RegisterProps> = ({ onSwitchToLogin, onRegisterSuccess }) => {
  const [formData, setFormData] = useState<RegisterRequest>({
    name: '',
    email: '',
    password: ''
  });
  const [confirmPassword, setConfirmPassword] = useState('');
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

  const handleConfirmPasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setConfirmPassword(e.target.value);
    if (error) setError('');
  };

  const validateForm = (): boolean => {
    if (!formData.name.trim()) {
      setError('Nome é obrigatório');
      return false;
    }
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
    if (formData.password !== confirmPassword) {
      setError('As senhas não coincidem');
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
      const response = await authService.register(formData);
      
      // Salvar token e dados do usuário
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      setSuccess('Registro realizado com sucesso!');
      
      // Aguardar um pouco para mostrar a mensagem de sucesso
      setTimeout(() => {
        onRegisterSuccess();
      }, 1000);
      
    } catch (err: any) {
      setError(err.message || 'Erro ao fazer registro');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <h1 className="form-title">Registro</h1>
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name" className="form-label">
            Nome Completo
          </label>
          <input
            type="text"
            id="name"
            name="name"
            className={`form-input ${error && !formData.name ? 'error' : ''}`}
            value={formData.name}
            onChange={handleInputChange}
            placeholder="Digite seu nome completo"
            disabled={loading}
          />
        </div>

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

        <div className="form-group">
          <label htmlFor="confirmPassword" className="form-label">
            Confirmar Senha
          </label>
          <input
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            className={`form-input ${error && formData.password !== confirmPassword ? 'error' : ''}`}
            value={confirmPassword}
            onChange={handleConfirmPasswordChange}
            placeholder="Confirme sua senha"
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
              Registrando...
            </>
          ) : (
            'Registrar'
          )}
        </button>
      </form>

      <div className="form-footer">
        <p>
          Já tem uma conta?{' '}
          <a href="#" onClick={onSwitchToLogin}>
            Faça login aqui
          </a>
        </p>
      </div>
    </div>
  );
};

export default Register; 