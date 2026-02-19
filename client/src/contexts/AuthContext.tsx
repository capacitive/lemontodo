import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import { authApi, accountApi } from '../api/authApi';
import { setAccessToken, getAccessToken } from '../api/client';
import type { UserProfile, LoginRequest, RegisterRequest } from '../types/auth';

interface AuthContextType {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (req: LoginRequest) => Promise<{ requiresTwoFactor: boolean }>;
  loginWithTokens: (accessToken: string, newRefreshToken: string) => Promise<void>;
  register: (req: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

let refreshToken: string | null = null;

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const login = useCallback(async (req: LoginRequest) => {
    const response = await authApi.login(req);
    if (response.requiresTwoFactor) {
      return { requiresTwoFactor: true };
    }
    setAccessToken(response.accessToken);
    refreshToken = response.refreshToken;
    setUser(response.user);
    return { requiresTwoFactor: false };
  }, []);

  const loginWithTokens = useCallback(async (accessToken: string, newRefreshToken: string) => {
    setAccessToken(accessToken);
    refreshToken = newRefreshToken;
    const profile = await accountApi.getProfile();
    setUser(profile);
  }, []);

  const register = useCallback(async (req: RegisterRequest) => {
    const response = await authApi.register(req);
    setAccessToken(response.accessToken);
    refreshToken = response.refreshToken;
    setUser(response.user);
  }, []);

  const logout = useCallback(async () => {
    if (refreshToken) {
      try {
        await authApi.logout({ refreshToken });
      } catch {
        // ignore logout errors
      }
    }
    setAccessToken(null);
    refreshToken = null;
    setUser(null);
  }, []);

  const refreshUser = useCallback(async () => {
    if (!refreshToken) return;
    try {
      const response = await authApi.refresh({ refreshToken });
      setAccessToken(response.accessToken);
      refreshToken = response.refreshToken;
      setUser(response.user);
    } catch {
      setAccessToken(null);
      refreshToken = null;
      setUser(null);
    }
  }, []);

  useEffect(() => {
    // On mount, check if we have a stored refresh token
    // For now, just mark loading as complete since we store tokens in memory only
    setIsLoading(false);
  }, []);

  // Auto-refresh access token before it expires (every 13 minutes)
  useEffect(() => {
    if (!user) return;
    const interval = setInterval(() => {
      refreshUser();
    }, 13 * 60 * 1000);
    return () => clearInterval(interval);
  }, [user, refreshUser]);

  return (
    <AuthContext.Provider value={{
      user,
      isAuthenticated: !!user && !!getAccessToken(),
      isLoading,
      login,
      loginWithTokens,
      register,
      logout,
      refreshUser,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
}
