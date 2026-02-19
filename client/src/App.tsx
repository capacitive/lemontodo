import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { Header } from './components/Layout/Header';
import { BoardView } from './components/Board/BoardView';
import { ListView } from './components/List/ListView';
import { ArchiveView } from './components/Archive/ArchiveView';
import { ProfileView } from './components/Account/ProfileView';
import { LoginView } from './components/Auth/LoginView';
import { RegisterView } from './components/Auth/RegisterView';
import { OAuthCallback } from './components/Auth/OAuthCallback';
import { useSignalR } from './hooks/useSignalR';
import type { ViewMode } from './types';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { refetchOnWindowFocus: false, retry: 1 },
  },
});

function AuthenticatedApp() {
  const [view, setView] = useState<ViewMode>('board');
  useSignalR();

  return (
    <div style={{ minHeight: '100vh', background: '#fafafa', fontFamily: 'system-ui, sans-serif' }}>
      <Header view={view} onViewChange={setView} />
      {view === 'board' && <BoardView />}
      {view === 'list' && <ListView />}
      {view === 'archive' && <ArchiveView />}
      {view === 'profile' && <ProfileView />}
    </div>
  );
}

function UnauthenticatedApp() {
  const [authView, setAuthView] = useState<'login' | 'register'>('login');

  if (authView === 'register') {
    return <RegisterView onSwitchToLogin={() => setAuthView('login')} />;
  }
  return <LoginView onSwitchToRegister={() => setAuthView('register')} />;
}

function AppContent() {
  const { isAuthenticated, isLoading } = useAuth();

  // Handle OAuth callback route
  if (window.location.pathname === '/auth/callback') {
    return <OAuthCallback />;
  }

  if (isLoading) {
    return (
      <div style={{
        display: 'flex', justifyContent: 'center', alignItems: 'center',
        minHeight: '100vh', color: '#6b7280',
      }}>
        Loading...
      </div>
    );
  }

  return isAuthenticated ? <AuthenticatedApp /> : <UnauthenticatedApp />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <AppContent />
        <Toaster position="bottom-right" richColors />
      </AuthProvider>
    </QueryClientProvider>
  );
}
