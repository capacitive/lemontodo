import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useAuth } from '../../contexts/AuthContext';
import { accountApi } from '../../api/authApi';
import { toast } from 'sonner';
import type { TwoFactorSetupResponse } from '../../types/auth';

export function ProfileView() {
  const { user, refreshUser } = useAuth();
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [totpSetup, setTotpSetup] = useState<TwoFactorSetupResponse | null>(null);
  const [totpCode, setTotpCode] = useState('');
  const [disableCode, setDisableCode] = useState('');
  const [apiKey, setApiKey] = useState<string | null>(null);
  const [loading, setLoading] = useState('');

  if (!user) return null;

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading('profile');
    try {
      await accountApi.updateProfile({ displayName });
      await refreshUser();
      toast.success('Profile updated');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Update failed');
    } finally {
      setLoading('');
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading('password');
    try {
      await accountApi.changePassword({ currentPassword, newPassword });
      setCurrentPassword('');
      setNewPassword('');
      toast.success('Password changed');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Password change failed');
    } finally {
      setLoading('');
    }
  };

  const handleSetup2fa = async () => {
    setLoading('2fa-setup');
    try {
      const setup = await accountApi.setup2fa();
      setTotpSetup(setup);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : '2FA setup failed');
    } finally {
      setLoading('');
    }
  };

  const handleEnable2fa = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading('2fa-enable');
    try {
      await accountApi.enable2fa(totpCode);
      setTotpSetup(null);
      setTotpCode('');
      await refreshUser();
      toast.success('Two-factor authentication enabled');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Verification failed');
    } finally {
      setLoading('');
    }
  };

  const handleDisable2fa = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading('2fa-disable');
    try {
      await accountApi.disable2fa(disableCode);
      setDisableCode('');
      await refreshUser();
      toast.success('Two-factor authentication disabled');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Verification failed');
    } finally {
      setLoading('');
    }
  };

  const handleGenerateApiKey = async () => {
    setLoading('api-key');
    try {
      const result = await accountApi.generateApiKey();
      setApiKey(result.apiKey);
      toast.success('API key generated');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to generate API key');
    } finally {
      setLoading('');
    }
  };

  const handleRevokeApiKey = async () => {
    setLoading('api-key-revoke');
    try {
      await accountApi.revokeApiKey();
      setApiKey(null);
      toast.success('API key revoked');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to revoke API key');
    } finally {
      setLoading('');
    }
  };

  return (
    <div style={{ maxWidth: 600, margin: '32px auto', padding: '0 16px' }}>
      <h2 style={{ color: '#374151', marginBottom: 24 }}>Account Settings</h2>

      {/* Profile Section */}
      <section style={sectionStyle}>
        <h3 style={sectionTitleStyle}>Profile</h3>
        <form onSubmit={handleUpdateProfile} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div>
            <label style={labelStyle}>Email</label>
            <input value={user.email} disabled style={{ ...inputStyle, background: '#f3f4f6' }} />
          </div>
          <div>
            <label style={labelStyle}>Display Name</label>
            <input
              value={displayName}
              onChange={e => setDisplayName(e.target.value)}
              maxLength={100}
              style={inputStyle}
            />
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <span style={{ fontSize: 13, color: '#9ca3af' }}>
              Member since {new Date(user.createdAt).toLocaleDateString()}
            </span>
          </div>
          <button type="submit" disabled={loading === 'profile'} style={primaryButtonStyle}>
            {loading === 'profile' ? 'Saving...' : 'Save'}
          </button>
        </form>
      </section>

      {/* Password Section */}
      <section style={sectionStyle}>
        <h3 style={sectionTitleStyle}>Change Password</h3>
        <form onSubmit={handleChangePassword} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div>
            <label style={labelStyle}>Current Password</label>
            <input
              type="password"
              value={currentPassword}
              onChange={e => setCurrentPassword(e.target.value)}
              required
              style={inputStyle}
            />
          </div>
          <div>
            <label style={labelStyle}>New Password</label>
            <input
              type="password"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              required
              minLength={8}
              style={inputStyle}
              placeholder="Min 8 chars, upper + lower + digit"
            />
          </div>
          <button type="submit" disabled={loading === 'password'} style={primaryButtonStyle}>
            {loading === 'password' ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      </section>

      {/* 2FA Section */}
      <section style={sectionStyle}>
        <h3 style={sectionTitleStyle}>Two-Factor Authentication</h3>
        {user.twoFactorEnabled ? (
          <div>
            <p style={{ color: '#16a34a', fontWeight: 500, marginBottom: 12 }}>
              2FA is enabled
            </p>
            <form onSubmit={handleDisable2fa} style={{ display: 'flex', gap: 8, alignItems: 'end' }}>
              <div style={{ flex: 1 }}>
                <label style={labelStyle}>Enter code to disable</label>
                <input
                  type="text"
                  value={disableCode}
                  onChange={e => setDisableCode(e.target.value)}
                  maxLength={6}
                  pattern="[0-9]{6}"
                  required
                  style={inputStyle}
                  placeholder="000000"
                />
              </div>
              <button type="submit" disabled={loading === '2fa-disable'} style={dangerButtonStyle}>
                Disable
              </button>
            </form>
          </div>
        ) : totpSetup ? (
          <div>
            <p style={{ color: '#374151', marginBottom: 12 }}>
              Scan this QR code with your authenticator app:
            </p>
            <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 16 }}>
              <QRCodeSVG value={totpSetup.qrCodeUri} size={200} />
            </div>
            <p style={{ fontSize: 12, color: '#6b7280', wordBreak: 'break-all', marginBottom: 16 }}>
              Manual entry key: <code style={{ background: '#f3f4f6', padding: '2px 4px', borderRadius: 3 }}>
                {totpSetup.sharedKey}
              </code>
            </p>
            <form onSubmit={handleEnable2fa} style={{ display: 'flex', gap: 8, alignItems: 'end' }}>
              <div style={{ flex: 1 }}>
                <label style={labelStyle}>Verification code</label>
                <input
                  type="text"
                  value={totpCode}
                  onChange={e => setTotpCode(e.target.value)}
                  maxLength={6}
                  pattern="[0-9]{6}"
                  required
                  style={{ ...inputStyle, textAlign: 'center', letterSpacing: 6 }}
                  placeholder="000000"
                  autoFocus
                />
              </div>
              <button type="submit" disabled={loading === '2fa-enable'} style={primaryButtonStyle}>
                Verify
              </button>
            </form>
          </div>
        ) : (
          <div>
            <p style={{ color: '#6b7280', marginBottom: 12 }}>
              Add an extra layer of security to your account with authenticator apps like
              Google Authenticator, Microsoft Authenticator, or Duo.
            </p>
            <button onClick={handleSetup2fa} disabled={loading === '2fa-setup'} style={primaryButtonStyle}>
              {loading === '2fa-setup' ? 'Setting up...' : 'Set up 2FA'}
            </button>
          </div>
        )}
      </section>

      {/* API Key Section */}
      <section style={sectionStyle}>
        <h3 style={sectionTitleStyle}>API Key</h3>
        <p style={{ color: '#6b7280', marginBottom: 12, fontSize: 14 }}>
          Use an API key for programmatic access to your tasks.
        </p>
        {apiKey && (
          <div style={{
            background: '#fef9c3', border: '1px solid #fbbf24', borderRadius: 6,
            padding: 12, marginBottom: 12, wordBreak: 'break-all', fontSize: 13,
          }}>
            <strong>Save this key — it won't be shown again:</strong>
            <br />
            <code>{apiKey}</code>
          </div>
        )}
        <div style={{ display: 'flex', gap: 8 }}>
          <button onClick={handleGenerateApiKey} disabled={loading === 'api-key'} style={primaryButtonStyle}>
            {loading === 'api-key' ? 'Generating...' : 'Generate New Key'}
          </button>
          <button onClick={handleRevokeApiKey} disabled={loading === 'api-key-revoke'} style={dangerButtonStyle}>
            Revoke Key
          </button>
        </div>
      </section>

      {/* Linked Providers Section */}
      <section style={sectionStyle}>
        <h3 style={sectionTitleStyle}>Linked Accounts</h3>
        {user.linkedProviders.length > 0 ? (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
            {user.linkedProviders.map(provider => (
              <li key={provider} style={{
                display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                padding: '8px 0', borderBottom: '1px solid #e5e7eb',
              }}>
                <span style={{ fontWeight: 500, color: '#374151' }}>{provider}</span>
                <span style={{ color: '#16a34a', fontSize: 13 }}>Connected</span>
              </li>
            ))}
          </ul>
        ) : (
          <p style={{ color: '#6b7280', fontSize: 14 }}>
            No external accounts linked. Sign in with Google or GitHub to link them.
          </p>
        )}
      </section>
    </div>
  );
}

const sectionStyle: React.CSSProperties = {
  background: '#fff', borderRadius: 8, padding: 24,
  marginBottom: 16, border: '1px solid #e5e7eb',
};

const sectionTitleStyle: React.CSSProperties = {
  margin: '0 0 16px', color: '#374151', fontSize: '1.1rem',
};

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

const dangerButtonStyle: React.CSSProperties = {
  padding: '10px 16px', borderRadius: 6, border: '1px solid #fca5a5',
  background: '#fff', color: '#dc2626', fontWeight: 500, fontSize: 14, cursor: 'pointer',
};
