import { useDroppable } from '@dnd-kit/core';
import type { TaskResponse, TodoTaskStatus } from '../../types';
import { DraggableCard } from './DraggableCard';

interface BoardColumnProps {
  status: TodoTaskStatus;
  label: string;
  color: string;
  tasks: TaskResponse[];
  onStart: (id: string) => void;
  onClose: (id: string) => void;
  onReopen: (id: string) => void;
  onEdit: (task: TaskResponse) => void;
}

export function BoardColumn({ status, label, color, tasks, onStart, onClose, onReopen, onEdit }: BoardColumnProps) {
  const { setNodeRef, isOver } = useDroppable({ id: status });

  return (
    <div
      ref={setNodeRef}
      style={{
        background: isOver ? '#fef9c3' : '#f9fafb',
        borderRadius: 12, padding: 16,
        border: isOver ? '2px dashed #fbbf24' : '2px solid transparent',
        transition: 'all 0.2s',
      }}
    >
      <div style={{
        display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12,
      }}>
        <div style={{
          width: 10, height: 10, borderRadius: '50%', background: color,
        }} />
        <h3 style={{ margin: 0, fontSize: '0.95rem', color: '#374151' }}>
          {label}
        </h3>
        <span style={{
          fontSize: '0.75rem', background: '#e5e7eb', borderRadius: 10,
          padding: '2px 8px', color: '#6b7280',
        }}>
          {tasks.length}
        </span>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {tasks.length === 0 && (
          <div style={{ color: '#9ca3af', fontSize: '0.85rem', textAlign: 'center', padding: 20 }}>
            Drop tasks here
          </div>
        )}
        {tasks.map((task) => (
          <DraggableCard
            key={task.id}
            task={task}
            onStart={() => onStart(task.id)}
            onClose={() => onClose(task.id)}
            onReopen={() => onReopen(task.id)}
            onEdit={() => onEdit(task)}
          />
        ))}
      </div>
    </div>
  );
}
