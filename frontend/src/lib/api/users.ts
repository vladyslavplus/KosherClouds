import { apiClient } from './client';

export interface UpdateUserRequest {
  userName?: string;
  email?: string;
  phoneNumber?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
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

export const usersApi = {
  getCurrentUser: async (): Promise<UserProfile> => {
    const response = await apiClient.get<UserProfile>('/users/me');
    return response.data;
  },

  updateUser: async (userId: string, data: UpdateUserRequest): Promise<UserProfile> => {
    const response = await apiClient.put<UserProfile>(`/users/${userId}`, data);
    return response.data;
  },

  changePassword: async (userId: string, data: ChangePasswordRequest): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>(`/users/${userId}/change-password`, data);
    return response.data;
  },
};