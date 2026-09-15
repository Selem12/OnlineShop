import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5206/api',
  withCredentials: true,
});

export const authApi = {
  register: (data) => api.post('/auth/register', data),
  login: (data) => api.post('/auth/login', data),
  logout: () => api.post('/auth/logout'),
  verifyEmail: (data) => api.post('/auth/verify-email', data),
  verifyPhone: (data) => api.post('/auth/verify-phone', data),
  getMe: () => api.get('/auth/me'),
};

export default api;
