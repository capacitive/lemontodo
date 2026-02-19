import type { ViewMode } from '../../types';

interface HeaderProps {
  view: ViewMode;
  onViewChange: (view: ViewMode) => void;
}

export function Header({ view, onViewChange }: HeaderProps) {
  return (
    <header style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '16px 24px', borderBottom: '1px solid #e5e7eb', background: '#fefce8',
    }}>
      <h1 style={{ margin: 0, fontSize: '1.5rem', color: '#854d0e' }}>
        Lemon Kanban
      </h1>
      <nav style={{ display: 'flex', gap: 8 }}>
        {(['board', 'list', 'archive'] as ViewMode[]).map((v) => (
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
      </nav>
    </header>
  );
}
