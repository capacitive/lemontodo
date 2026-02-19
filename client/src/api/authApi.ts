import { api } from './client';
import type {
  RegisterRequest,
  LoginRequest,
  AuthResponse,
  UserProfile,
  UpdateProfileRequest,
  ChangePasswordRequest,
  TwoFactorSetupResponse,
  ApiKeyResponse,
  RefreshTokenRequest,
} from '../types/auth';

const BASE_URL = 'http://localhost:5175/api';

export const authApi = {
  register: (req: RegisterRequest) =>
    api.post<AuthResponse>('/auth/register', req),

  login: (req: LoginRequest) =>
    api.post<AuthResponse>('/auth/login', req),

  refresh: (req: RefreshTokenRequest) =>
    api.post<AuthResponse>('/auth/refresh', req),

  logout: (req: RefreshTokenRequest) =>
    api.post<void>('/auth/logout', req),

  googleLogin: () => {
    window.location.href = `${BASE_URL}/auth/google`;
  },

  githubLogin: () => {
    window.location.href = `${BASE_URL}/auth/github`;
  },
};

export const accountApi = {
  getProfile: () =>
    api.get<UserProfile>('/account/profile'),

  updateProfile: (req: UpdateProfileRequest) =>
    api.put<UserProfile>('/account/profile', req),

  changePassword: (req: ChangePasswordRequest) =>
    api.post<void>('/account/change-password', req),

  setup2fa: () =>
    api.post<TwoFactorSetupResponse>('/account/2fa/setup', {}),

  enable2fa: (code: string) =>
    api.post<void>('/account/2fa/enable', { code }),

  disable2fa: (code: string) =>
    api.post<void>('/account/2fa/disable', { code }),

  generateApiKey: () =>
    api.post<ApiKeyResponse>('/account/api-key', {}),

  revokeApiKey: () =>
    api.delete<void>('/account/api-key'),

  getPreferences: () =>
    api.get<{ preferences: string | null }>('/account/preferences'),

  updatePreferences: (preferences: string | null) =>
    api.put<{ preferences: string | null }>('/account/preferences', { preferences }),
};
