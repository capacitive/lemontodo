import type { TaskResponse } from '../../types';

interface TaskCardProps {
  task: TaskResponse;
  onClose?: () => void;
  onReopen?: () => void;
  onEdit?: () => void;
  draggable?: boolean;
}

export function TaskCard({ task, onClose, onReopen, onEdit, draggable }: TaskCardProps) {
  const statusColors: Record<string, string> = {
    Open: '#16a34a',
    Closed: '#dc2626',
    Reopened: '#2563eb',
  };

  return (
    <div
      style={{
        padding: 12, borderRadius: 8, background: '#fff',
        border: '1px solid #e5e7eb', cursor: draggable ? 'grab' : 'default',
        boxShadow: '0 1px 2px rgba(0,0,0,0.05)',
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: 4 }}>
        <strong style={{ fontSize: '0.95rem' }}>{task.name}</strong>
        <span style={{
          fontSize: '0.7rem', padding: '2px 6px', borderRadius: 4,
          background: statusColors[task.status] + '18',
          color: statusColors[task.status],
          fontWeight: 600,
        }}>
          {task.status}
        </span>
      </div>

      {task.description && (
        <p style={{ margin: '4px 0', fontSize: '0.85rem', color: '#6b7280' }}>
          {task.description}
        </p>
      )}

      <div style={{ fontSize: '0.75rem', color: '#9ca3af', marginTop: 6 }}>
        Due: {task.completionDate}
      </div>

      <div style={{ display: 'flex', gap: 6, marginTop: 8 }}>
        {onEdit && task.status !== 'Closed' && (
          <button onClick={onEdit} style={btnStyle('#f3f4f6', '#374151')}>Edit</button>
        )}
        {task.status !== 'Closed' && onClose && (
          <button onClick={onClose} style={btnStyle('#fee2e2', '#dc2626')}>Close</button>
        )}
        {task.status === 'Closed' && onReopen && (
          <button onClick={onReopen} style={btnStyle('#dbeafe', '#2563eb')}>Reopen</button>
        )}
      </div>
    </div>
  );
}

function btnStyle(bg: string, color: string): React.CSSProperties {
  return {
    padding: '4px 10px', borderRadius: 4, border: 'none',
    background: bg, color, fontSize: '0.75rem', cursor: 'pointer', fontWeight: 500,
  };
}
