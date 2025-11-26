import { apiClient } from './client';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  userName: string;
  phoneNumber: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

export interface UserPublicInfo {
  id: string;
  userName: string | null;
  email: string | null;
  firstName?: string | null;
  lastName?: string | null;
  displayName: string;
}

export interface UserProfile {
  id: string;
  userName: string | null;
  email: string | null;
  phoneNumber?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  createdAt: string;
  updatedAt?: string | null;
  displayName: string;
}

export const authApi = {
  register: async (data: RegisterRequest): Promise<{ tokens: TokenResponse; user: UserProfile }> => {
    const response = await apiClient.post<TokenResponse>('/auth/register', data);
    
    const userResponse = await apiClient.get<UserProfile>('/users/me', {
      headers: {
        Authorization: `Bearer ${response.data.accessToken}`,
      },
    });

    return {
      tokens: response.data,
      user: userResponse.data,
    };
  },

  login: async (data: LoginRequest): Promise<{ tokens: TokenResponse; user: UserProfile }> => {
    const response = await apiClient.post<TokenResponse>('/auth/login', data);
    
    const userResponse = await apiClient.get<UserProfile>('/users/me', {
      headers: {
        Authorization: `Bearer ${response.data.accessToken}`,
      },
    });

    return {
      tokens: response.data,
      user: userResponse.data,
    };
  },

  logout: async () => {
    await apiClient.post('/tokens/revoke');
  },

  forgotPassword: async (data: ForgotPasswordRequest) => {
    const response = await apiClient.post<{ message: string }>('/auth/forgot-password', data);
    return response.data;
  },

  resetPassword: async (data: ResetPasswordRequest) => {
    const response = await apiClient.post<{ message: string }>('/auth/reset-password', data);
    return response.data;
  },

  getCurrentUser: async (): Promise<UserProfile> => {
    const response = await apiClient.get<UserProfile>('/users/me');
    return response.data;
  },

  getUserPublicInfo: async (userId: string): Promise<UserPublicInfo> => {
    const response = await apiClient.get<UserPublicInfo>(`/users/${userId}/public`);
    return response.data;
  },
};