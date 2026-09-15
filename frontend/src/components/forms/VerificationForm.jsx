import React, { useState } from 'react';
import { authApi } from '../../services/api';
import { useNavigate } from 'react-router-dom';

export default function VerificationForm({ type = 'email' }) {
  const [code, setCode] = useState('');
  const [identifier, setIdentifier] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const apiCall = type === 'email' 
        ? authApi.verifyEmail({ email: identifier, code }) 
        : authApi.verifyPhone({ phoneNumber: identifier, code });
      
      const { data } = await apiCall;
      if (data.success) {
        alert(`${type} verified successfully!`);
        navigate('/dashboard');
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('An unexpected error occurred');
    }
  };

  return (
    <form onSubmit={handleSubmit} className="auth-form">
      <h2>Verify {type === 'email' ? 'Email' : 'Phone'}</h2>
      {error && <p className="error">{error}</p>}
      <div className="form-group">
        <label>{type === 'email' ? 'Email' : 'Phone Number'}</label>
        <input 
          type="text" 
          value={identifier} 
          onChange={e => setIdentifier(e.target.value)} 
          required 
        />
      </div>
      <div className="form-group">
        <label>Verification Code</label>
        <input 
          type="text" 
          value={code} 
          onChange={e => setCode(e.target.value)} 
          placeholder="123456" 
          required 
        />
      </div>
      <button type="submit">Verify</button>
    </form>
  );
}
