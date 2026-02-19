import { useAuth } from '../../contexts/AuthContext';
import type { ViewMode } from '../../types';

interface HeaderProps {
  view: ViewMode;
  onViewChange: (view: ViewMode) => void;
}

const navViews: ViewMode[] = ['board', 'list', 'archive'];

export function Header({ view, onViewChange }: HeaderProps) {
  const { user, logout } = useAuth();

  return (
    <header style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '16px 24px', borderBottom: '1px solid #e5e7eb', background: '#fefce8',
    }}>
      <h1 style={{ margin: 0, fontSize: '1.5rem', color: '#854d0e' }}>
        Lemon Kanban
      </h1>
      <nav style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        {navViews.map((v) => (
          <button
            key={v}
            onClick={() => onViewChange(v)}
            style={{
              padding: '6px 16px', borderRadius: 6, border: '1px solid #d4d4d8',
              background: view === v ? '#fbbf24' : '#fff',
              color: view === v ? '#78350f' : '#52525b',
              fontWeight: view === v ? 600 : 400,
              cursor: 'pointer', textTransform: 'capitalize',
            }}
          >
            {v}
          </button>
        ))}
        <div style={{ width: 1, height: 24, background: '#d4d4d8', margin: '0 4px' }} />
        <button
          onClick={() => onViewChange('profile')}
          style={{
            padding: '6px 16px', borderRadius: 6, border: '1px solid #d4d4d8',
            background: view === 'profile' ? '#fbbf24' : '#fff',
            color: view === 'profile' ? '#78350f' : '#52525b',
            fontWeight: view === 'profile' ? 600 : 400,
            cursor: 'pointer',
          }}
        >
          {user?.displayName ?? 'Profile'}
        </button>
        <button
          onClick={logout}
          style={{
            padding: '6px 16px', borderRadius: 6, border: '1px solid #fca5a5',
            background: '#fff', color: '#dc2626',
            fontWeight: 500, cursor: 'pointer',
          }}
        >
          Logout
        </button>
      </nav>
    </header>
  );
}
