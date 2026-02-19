import { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { toast } from 'sonner';

interface RegisterViewProps {
  onSwitchToLogin: () => void;
}

export function RegisterView({ onSwitchToLogin }: RegisterViewProps) {
  const { register } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await register({ email, password, displayName });
      toast.success('Account created successfully');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Registration failed');
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
          Create your account
        </p>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div>
            <label style={labelStyle}>Display Name</label>
            <input
              type="text"
              value={displayName}
              onChange={e => setDisplayName(e.target.value)}
              required
              maxLength={100}
              style={inputStyle}
              placeholder="Your name"
            />
          </div>
          <div>
            <label style={labelStyle}>Email</label>
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
            <label style={labelStyle}>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              minLength={8}
              style={inputStyle}
              placeholder="Min 8 chars, upper + lower + digit"
            />
          </div>

          <button type="submit" disabled={loading} style={primaryButtonStyle}>
            {loading ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p style={{ marginTop: 24, textAlign: 'center', fontSize: 14, color: '#6b7280' }}>
          Already have an account?{' '}
          <button onClick={onSwitchToLogin} style={linkButtonStyle}>
            Sign in
          </button>
        </p>
      </div>
    </div>
  );
}

const labelStyle: React.CSSProperties = {
  display: 'block', marginBottom: 4, fontSize: 14, color: '#374151',
};

const inputStyle: React.CSSProperties = {
  width: '100%', padding: '10px 12px', borderRadius: 6,
  border: '1px solid #d4d4d8', fontSize: 14, outline: 'none', boxSizing: 'border-box',
};

const primaryButtonStyle: React.CSSProperties = {
  padding: '10px 16px', borderRadius: 6, border: 'none',
  background: '#fbbf24', color: '#78350f', fontWeight: 600, fontSize: 14, cursor: 'pointer',
};

const linkButtonStyle: React.CSSProperties = {
  background: 'none', border: 'none', color: '#d97706',
  fontWeight: 600, cursor: 'pointer', padding: 0, fontSize: 14,
};
