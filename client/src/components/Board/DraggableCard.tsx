import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import type { TaskResponse } from '../../types';
import { TaskCard } from '../Task/TaskCard';

interface DraggableCardProps {
  task: TaskResponse;
  onClose: () => void;
  onReopen: () => void;
  onEdit: () => void;
}

export function DraggableCard({ task, onClose, onReopen, onEdit }: DraggableCardProps) {
  const { attributes, listeners, setNodeRef, transform } = useDraggable({
    id: task.id,
  });

  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Translate.toString(transform) }}
      {...listeners}
      {...attributes}
    >
      <TaskCard task={task} onClose={onClose} onReopen={onReopen} onEdit={onEdit} draggable />
    </div>
  );
}
