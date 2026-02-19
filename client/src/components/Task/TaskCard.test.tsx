import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../../test/utils';
import { TaskCard } from './TaskCard';
import type { TaskResponse } from '../../types';

const mockTask: TaskResponse = {
  id: 'task-1',
  name: 'Test Task',
  description: 'Test description',
  completionDate: '2026-03-01',
  status: 'Open',
  createdAt: '2026-02-16T10:00:00Z',
  closedAt: null,
  reopenedAt: null,
};

describe('TaskCard', () => {
  it('should render task details', () => {
    render(<TaskCard task={mockTask} />);

    expect(screen.getByText('Test Task')).toBeInTheDocument();
    expect(screen.getByText('Test description')).toBeInTheDocument();
    expect(screen.getByText(/2026-03-01/)).toBeInTheDocument();
    expect(screen.getByText('Open')).toBeInTheDocument();
  });

  it('should call onEdit when Edit button clicked', async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();

    render(<TaskCard task={mockTask} onEdit={onEdit} />);

    await user.click(screen.getByText('Edit'));

    expect(onEdit).toHaveBeenCalled();
  });

  it('should display status badges', () => {
    const { rerender } = render(<TaskCard task={mockTask} />);
    expect(screen.getByText('Open')).toBeInTheDocument();

    rerender(<TaskCard task={{ ...mockTask, status: 'Closed' }} />);
    expect(screen.getByText('Closed')).toBeInTheDocument();

    rerender(<TaskCard task={{ ...mockTask, status: 'Reopened' }} />);
    expect(screen.getByText('Reopened')).toBeInTheDocument();
  });

  it('should handle task without description', () => {
    render(<TaskCard task={{ ...mockTask, description: null }} />);

    expect(screen.getByText('Test Task')).toBeInTheDocument();
    expect(screen.queryByText('Test description')).not.toBeInTheDocument();
  });

  it('should not show Edit button for closed tasks even when onEdit provided', () => {
    const onEdit = vi.fn();
    const closedTask = { ...mockTask, status: 'Closed' as const, closedAt: '2026-02-17T10:00:00Z' };

    render(<TaskCard task={closedTask} onEdit={onEdit} />);

    expect(screen.queryByText('Edit')).not.toBeInTheDocument();
  });

  it('should show Reopen button for closed tasks', async () => {
    const user = userEvent.setup();
    const onReopen = vi.fn();
    const closedTask = { ...mockTask, status: 'Closed' as const, closedAt: '2026-02-17T10:00:00Z' };

    render(<TaskCard task={closedTask} onReopen={onReopen} />);

    expect(screen.getByText('Reopen')).toBeInTheDocument();
    await user.click(screen.getByText('Reopen'));
    expect(onReopen).toHaveBeenCalled();
  });

  it('should not show Close button for closed tasks', () => {
    const onClose = vi.fn();
    const closedTask = { ...mockTask, status: 'Closed' as const, closedAt: '2026-02-17T10:00:00Z' };

    render(<TaskCard task={closedTask} onClose={onClose} />);

    expect(screen.queryByText('Close')).not.toBeInTheDocument();
  });

  it('should show Edit button for open and reopened tasks', () => {
    const onEdit = vi.fn();

    const { rerender } = render(<TaskCard task={mockTask} onEdit={onEdit} />);
    expect(screen.getByText('Edit')).toBeInTheDocument();

    rerender(<TaskCard task={{ ...mockTask, status: 'Reopened' }} onEdit={onEdit} />);
    expect(screen.getByText('Edit')).toBeInTheDocument();
  });
});
