import { useEffect, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { toast } from 'sonner';

export function OAuthCallback() {
  const { loginWithTokens } = useAuth();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const hash = window.location.hash.substring(1); // Remove the #
    const params = new URLSearchParams(hash);
    const accessToken = params.get('access_token');
    const refreshToken = params.get('refresh_token');

    if (!accessToken || !refreshToken) {
      setError('Missing authentication tokens. Please try again.');
      return;
    }

    // Clear the hash from URL to avoid token exposure in browser history
    window.history.replaceState(null, '', '/');

    loginWithTokens(accessToken, refreshToken)
      .then(() => {
        toast.success('Signed in with GitHub');
      })
      .catch(() => {
        setError('Failed to complete sign-in. Please try again.');
      });
  }, [loginWithTokens]);

  if (error) {
    return (
      <div style={{
        display: 'flex', justifyContent: 'center', alignItems: 'center',
        minHeight: '100vh', background: '#fafafa', fontFamily: 'system-ui, sans-serif',
      }}>
        <div style={{
          background: '#fff', borderRadius: 12, padding: '40px 32px',
          boxShadow: '0 4px 24px rgba(0,0,0,0.08)', maxWidth: 400, width: '100%',
          textAlign: 'center',
        }}>
          <p style={{ color: '#dc2626', marginBottom: 16 }}>{error}</p>
          <a href="/" style={{ color: '#d97706', fontWeight: 600 }}>Back to login</a>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      display: 'flex', justifyContent: 'center', alignItems: 'center',
      minHeight: '100vh', color: '#6b7280',
    }}>
      Completing sign-in...
    </div>
  );
}
