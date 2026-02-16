import { useState, useEffect } from 'react';
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest } from '../../types';

interface TaskModalProps {
  task?: TaskResponse | null;
  onClose: () => void;
  onCreate?: (req: CreateTaskRequest) => void;
  onUpdate?: (id: string, req: UpdateTaskRequest) => void;
}

export function TaskModal({ task, onClose, onCreate, onUpdate }: TaskModalProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [completionDate, setCompletionDate] = useState('');

  useEffect(() => {
    if (task) {
      setName(task.name);
      setDescription(task.description ?? '');
      setCompletionDate(task.completionDate);
    } else {
      setName('');
      setDescription('');
      setCompletionDate(new Date().toISOString().split('T')[0]);
    }
  }, [task]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      name,
      description: description || null,
      completionDate,
    };

    if (task && onUpdate) {
      onUpdate(task.id, payload);
    } else if (onCreate) {
      onCreate(payload);
    }
    onClose();
  };

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
    }} onClick={onClose}>
      <form
        onClick={(e) => e.stopPropagation()}
        onSubmit={handleSubmit}
        style={{
          background: '#fff', borderRadius: 12, padding: 24, width: 420,
          boxShadow: '0 20px 60px rgba(0,0,0,0.15)',
        }}
      >
        <h2 style={{ margin: '0 0 16px', fontSize: '1.2rem' }}>
          {task ? 'Edit Task' : 'New Task'}
        </h2>

        <label style={labelStyle}>Name</label>
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          maxLength={200}
          style={inputStyle}
          autoFocus
        />

        <label style={labelStyle}>Description</label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          maxLength={2000}
          rows={3}
          style={inputStyle}
        />

        <label style={labelStyle}>Completion Date</label>
        <input
          type="date"
          value={completionDate}
          onChange={(e) => setCompletionDate(e.target.value)}
          required
          style={inputStyle}
        />

        <div style={{ display: 'flex', gap: 8, marginTop: 16, justifyContent: 'flex-end' }}>
          <button type="button" onClick={onClose} style={{
            padding: '8px 16px', borderRadius: 6, border: '1px solid #d4d4d8',
            background: '#fff', cursor: 'pointer',
          }}>Cancel</button>
          <button type="submit" style={{
            padding: '8px 16px', borderRadius: 6, border: 'none',
            background: '#fbbf24', color: '#78350f', fontWeight: 600, cursor: 'pointer',
          }}>
            {task ? 'Update' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}

const labelStyle: React.CSSProperties = {
  display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: 4, marginTop: 12, color: '#374151',
};

const inputStyle: React.CSSProperties = {
  width: '100%', padding: '8px 12px', borderRadius: 6, border: '1px solid #d4d4d8',
  fontSize: '0.9rem', boxSizing: 'border-box',
};
