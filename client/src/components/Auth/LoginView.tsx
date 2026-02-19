import { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { authApi } from '../../api/authApi';
import { toast } from 'sonner';

interface LoginViewProps {
  onSwitchToRegister: () => void;
}

export function LoginView({ onSwitchToRegister }: LoginViewProps) {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [showTotp, setShowTotp] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const result = await login({ email, password, totpCode: showTotp ? totpCode : undefined });
      if (result.requiresTwoFactor) {
        setShowTotp(true);
        toast.info('Enter your authenticator code');
      }
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      display: 'flex', justifyContent: 'center', alignItems: 'center',
      minHeight: '100vh', background: '#fafafa', fontFamily: 'system-ui, sans-serif',
    }}>
      <div style={{
        background: '#fff', borderRadius: 12, padding: '40px 32px',
        boxShadow: '0 4px 24px rgba(0,0,0,0.08)', maxWidth: 400, width: '100%',
      }}>
        <h1 style={{ margin: '0 0 8px', fontSize: '1.5rem', color: '#854d0e', textAlign: 'center' }}>
          LemonTodo
        </h1>
        <p style={{ margin: '0 0 24px', color: '#6b7280', textAlign: 'center' }}>
          Sign in to your account
        </p>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div>
            <label style={{ display: 'block', marginBottom: 4, fontSize: 14, color: '#374151' }}>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              style={inputStyle}
              placeholder="you@example.com"
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: 4, fontSize: 14, color: '#374151' }}>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              style={inputStyle}
              placeholder="Enter your password"
            />
          </div>

          {showTotp && (
            <div>
              <label style={{ display: 'block', marginBottom: 4, fontSize: 14, color: '#374151' }}>
                Authenticator Code
              </label>
              <input
                type="text"
                value={totpCode}
                onChange={e => setTotpCode(e.target.value)}
                maxLength={6}
                pattern="[0-9]{6}"
                required
                style={{ ...inputStyle, textAlign: 'center', letterSpacing: 8, fontSize: 20 }}
                placeholder="000000"
                autoFocus
              />
            </div>
          )}

          <button type="submit" disabled={loading} style={primaryButtonStyle}>
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <div style={{ margin: '20px 0', textAlign: 'center', color: '#9ca3af', fontSize: 14 }}>
          or continue with
        </div>

        <div style={{ display: 'flex', gap: 8 }}>
          <button onClick={() => authApi.googleLogin()} style={oauthButtonStyle}>
            Google
          </button>
          <button onClick={() => authApi.githubLogin()} style={oauthButtonStyle}>
            GitHub
          </button>
        </div>

        <p style={{ marginTop: 24, textAlign: 'center', fontSize: 14, color: '#6b7280' }}>
          Don't have an account?{' '}
          <button onClick={onSwitchToRegister} style={linkButtonStyle}>
            Sign up
          </button>
        </p>
      </div>
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  width: '100%',
  padding: '10px 12px',
  borderRadius: 6,
  border: '1px solid #d4d4d8',
  fontSize: 14,
  outline: 'none',
  boxSizing: 'border-box',
};

const primaryButtonStyle: React.CSSProperties = {
  padding: '10px 16px',
  borderRadius: 6,
  border: 'none',
  background: '#fbbf24',
  color: '#78350f',
  fontWeight: 600,
  fontSize: 14,
  cursor: 'pointer',
};

const oauthButtonStyle: React.CSSProperties = {
  flex: 1,
  padding: '10px 16px',
  borderRadius: 6,
  border: '1px solid #d4d4d8',
  background: '#fff',
  color: '#374151',
  fontWeight: 500,
  fontSize: 14,
  cursor: 'pointer',
};

const linkButtonStyle: React.CSSProperties = {
  background: 'none',
  border: 'none',
  color: '#d97706',
  fontWeight: 600,
  cursor: 'pointer',
  padding: 0,
  fontSize: 14,
};
