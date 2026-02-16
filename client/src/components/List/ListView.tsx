import { useState } from 'react';
import { useTasks, useCloseTask, useReopenTask, useCreateTask, useUpdateTask } from '../../hooks/useTasks';
import { TaskCard } from '../Task/TaskCard';
import { TaskModal } from '../Task/TaskModal';
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest } from '../../types';

export function ListView() {
  const { data: tasks, isLoading, error } = useTasks();
  const closeTask = useCloseTask();
  const reopenTask = useReopenTask();
  const createTask = useCreateTask();
  const updateTask = useUpdateTask();
  const [showModal, setShowModal] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskResponse | null>(null);

  if (isLoading) return <div style={{ padding: 24, textAlign: 'center' }}>Loading...</div>;
  if (error) return <div style={{ padding: 24, color: '#dc2626' }}>Error loading tasks</div>;

  const active = tasks?.filter((t) => t.status !== 'Closed') ?? [];
  const recentlyClosed = tasks?.filter((t) => t.status === 'Closed') ?? [];

  return (
    <div style={{ padding: 24, maxWidth: 700, margin: '0 auto' }}>
      <button
        onClick={() => { setEditingTask(null); setShowModal(true); }}
        style={{
          padding: '8px 20px', borderRadius: 6, border: 'none',
          background: '#fbbf24', color: '#78350f', fontWeight: 600,
          cursor: 'pointer', fontSize: '0.9rem', marginBottom: 20,
        }}
      >
        + New Task
      </button>

      <h2 style={{ fontSize: '1.1rem', color: '#374151', marginBottom: 12 }}>
        Active Tasks ({active.length})
      </h2>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8, marginBottom: 24 }}>
        {active.length === 0 && (
          <div style={{ color: '#9ca3af', fontSize: '0.9rem' }}>No active tasks</div>
        )}
        {active.map((task) => (
          <TaskCard
            key={task.id}
            task={task}
            onClose={() => closeTask.mutate(task.id)}
            onReopen={() => reopenTask.mutate(task.id)}
            onEdit={() => { setEditingTask(task); setShowModal(true); }}
          />
        ))}
      </div>

      {recentlyClosed.length > 0 && (
        <>
          <h2 style={{ fontSize: '1.1rem', color: '#374151', marginBottom: 12 }}>
            Recently Closed ({recentlyClosed.length})
          </h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            {recentlyClosed.map((task) => (
              <TaskCard
                key={task.id}
                task={task}
                onReopen={() => reopenTask.mutate(task.id)}
              />
            ))}
          </div>
        </>
      )}

      {showModal && (
        <TaskModal
          task={editingTask}
          onClose={() => setShowModal(false)}
          onCreate={(req: CreateTaskRequest) => createTask.mutate(req)}
          onUpdate={(id: string, req: UpdateTaskRequest) => updateTask.mutate({ id, req })}
        />
      )}
    </div>
  );
}
