import { useState } from 'react';
import { DndContext, closestCenter } from '@dnd-kit/core';
import type { TaskResponse, TodoTaskStatus } from '../../types';
import { useTasks, useCloseTask, useReopenTask, useCreateTask, useUpdateTask } from '../../hooks/useTasks';
import { useArchiveSearch, useRestoreTask } from '../../hooks/useArchive';
import { BoardColumn } from './BoardColumn';
import { TaskModal } from '../Task/TaskModal';
import type { CreateTaskRequest, UpdateTaskRequest } from '../../types';

const columns: { status: TodoTaskStatus; label: string; color: string }[] = [
  { status: 'Open', label: 'Open', color: '#16a34a' },
  { status: 'Reopened', label: 'Reopened', color: '#2563eb' },
  { status: 'Closed', label: 'Closed', color: '#dc2626' },
];

export function BoardView() {
  const { data: tasks, isLoading, error } = useTasks();
  const { data: archiveData } = useArchiveSearch('', 1, 100);
  const closeTask = useCloseTask();
  const reopenTask = useReopenTask();
  const restoreTask = useRestoreTask();
  const createTask = useCreateTask();
  const updateTask = useUpdateTask();
  const [showModal, setShowModal] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskResponse | null>(null);

  const archivedTasks = archiveData?.items ?? [];
  const archivedIds = new Set(archivedTasks.map((t) => t.id));

  const handleReopen = (id: string) => {
    if (archivedIds.has(id)) {
      restoreTask.mutate(id);
    } else {
      reopenTask.mutate(id);
    }
  };

  const handleDragEnd = (event: { active: { id: string | number }; over: { id: string | number } | null }) => {
    const { active, over } = event;
    if (!over || !tasks) return;

    const taskId = active.id as string;
    const targetStatus = over.id as TodoTaskStatus;
    const task = tasks.find((t) => t.id === taskId);
    if (!task || task.status === targetStatus) return;

    if (targetStatus === 'Closed' && (task.status === 'Open' || task.status === 'Reopened')) {
      closeTask.mutate(taskId);
    } else if (targetStatus === 'Reopened' && task.status === 'Closed') {
      handleReopen(taskId);
    }
  };

  if (isLoading) return <div style={{ padding: 24, textAlign: 'center' }}>Loading...</div>;
  if (error) return <div style={{ padding: 24, color: '#dc2626' }}>Error loading tasks</div>;

  const byDueDate = (a: TaskResponse, b: TaskResponse) =>
    a.completionDate.localeCompare(b.completionDate);

  const grouped = Object.fromEntries(
    columns.map(({ status }) => {
      const active = tasks?.filter((t) => t.status === status) ?? [];
      if (status === 'Closed') {
        return [status, [...active, ...archivedTasks].sort(byDueDate)];
      }
      return [status, [...active].sort(byDueDate)];
    })
  ) as Record<TodoTaskStatus, TaskResponse[]>;

  return (
    <>
      <div style={{ padding: '16px 24px' }}>
        <button
          onClick={() => { setEditingTask(null); setShowModal(true); }}
          style={{
            padding: '8px 20px', borderRadius: 6, border: 'none',
            background: '#fbbf24', color: '#78350f', fontWeight: 600,
            cursor: 'pointer', fontSize: '0.9rem',
          }}
        >
          + New Task
        </button>
      </div>

      <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <div style={{
          display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)',
          gap: 16, padding: '0 24px 24px', minHeight: 'calc(100vh - 160px)',
        }}>
          {columns.map(({ status, label, color }) => (
            <BoardColumn
              key={status}
              status={status}
              label={label}
              color={color}
              tasks={grouped[status]}
              onClose={(id) => closeTask.mutate(id)}
              onReopen={handleReopen}
              onEdit={(task) => { setEditingTask(task); setShowModal(true); }}
            />
          ))}
        </div>
      </DndContext>

      {showModal && (
        <TaskModal
          task={editingTask}
          onClose={() => setShowModal(false)}
          onCreate={(req: CreateTaskRequest) => createTask.mutate(req)}
          onUpdate={(id: string, req: UpdateTaskRequest) => updateTask.mutate({ id, req })}
        />
      )}
    </>
  );
}
