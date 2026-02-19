export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  totpCode?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserProfile;
  requiresTwoFactor: boolean;
}

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  twoFactorEnabled: boolean;
  linkedProviders: string[];
  createdAt: string;
  lastLoginAt: string | null;
}

export interface TwoFactorSetupResponse {
  sharedKey: string;
  qrCodeUri: string;
}

export interface UpdateProfileRequest {
  displayName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ApiKeyResponse {
  apiKey: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}
