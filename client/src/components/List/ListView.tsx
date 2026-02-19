import { useState } from 'react';
import { useTasks, useCloseTask, useReopenTask, useCreateTask, useUpdateTask } from '../../hooks/useTasks';
import { useArchiveSearch, useRestoreTask } from '../../hooks/useArchive';
import { TaskCard } from '../Task/TaskCard';
import { TaskModal } from '../Task/TaskModal';
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest } from '../../types';

export function ListView() {
  const { data: tasks, isLoading, error } = useTasks();
  const { data: archiveData } = useArchiveSearch('', 1, 100);
  const closeTask = useCloseTask();
  const reopenTask = useReopenTask();
  const restoreTask = useRestoreTask();
  const createTask = useCreateTask();
  const updateTask = useUpdateTask();
  const [showModal, setShowModal] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskResponse | null>(null);

  if (isLoading) return <div style={{ padding: 24, textAlign: 'center' }}>Loading...</div>;
  if (error) return <div style={{ padding: 24, color: '#dc2626' }}>Error loading tasks</div>;

  const archivedTasks = archiveData?.items ?? [];
  const archivedIds = new Set(archivedTasks.map((t) => t.id));

  const handleReopen = (id: string) => {
    if (archivedIds.has(id)) {
      restoreTask.mutate(id);
    } else {
      reopenTask.mutate(id);
    }
  };

  const active = tasks?.filter((t) => t.status !== 'Closed') ?? [];
  const recentlyClosed = [
    ...(tasks?.filter((t) => t.status === 'Closed') ?? []),
    ...archivedTasks,
  ];

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
          <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {recentlyClosed.map((task) => (
              <div
                key={task.id}
                style={{
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                  padding: '6px 10px', borderRadius: 6, fontSize: '0.85rem',
                  color: '#6b7280', background: '#f9fafb',
                }}
              >
                <span style={{ textDecoration: 'line-through' }}>{task.name}</span>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexShrink: 0 }}>
                  <span style={{ fontSize: '0.75rem', color: '#9ca3af' }}>
                    {task.closedAt ? new Date(task.closedAt).toLocaleDateString() : ''}
                  </span>
                  <button
                    onClick={() => handleReopen(task.id)}
                    style={{
                      padding: '2px 8px', borderRadius: 4, border: 'none',
                      background: '#dbeafe', color: '#2563eb', fontSize: '0.75rem',
                      cursor: 'pointer', fontWeight: 500,
                    }}
                  >
                    Reopen
                  </button>
                </div>
              </div>
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
