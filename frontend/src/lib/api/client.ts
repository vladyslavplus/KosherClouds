import axios, { type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../stores/authStore';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const { accessToken } = useAuthStore.getState();
    
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      error.response?.status === 401 && 
      !originalRequest._retry &&
      !originalRequest.url?.includes('/tokens/refresh')
    ) {
      if (originalRequest.url?.includes('/auth/login') || originalRequest.url?.includes('/auth/register')) {
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      const { accessToken, clearAuth } = useAuthStore.getState();

      if (accessToken) {
        try {
          const response = await axios.post(
            `${API_BASE_URL}/tokens/refresh`,
            {},
            {
              headers: {
                Authorization: `Bearer ${accessToken}`,
              },
            }
          );

          const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;
          
          const { user } = useAuthStore.getState();
          if (user) {
            useAuthStore.getState().setAuth(newAccessToken, newRefreshToken, user);
          }

          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          return apiClient(originalRequest);
        } catch (refreshError) {
          clearAuth();
          if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
            window.location.href = '/login';
          }
          return Promise.reject(refreshError);
        }
      } else {
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);