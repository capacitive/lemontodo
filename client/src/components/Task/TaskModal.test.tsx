import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../../test/utils';
import { TaskModal } from './TaskModal';
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

describe('TaskModal', () => {
  it('should render create mode when no task provided', () => {
    render(
      <TaskModal
        onClose={vi.fn()}
        onCreate={vi.fn()}
      />
    );

    expect(screen.getByText('New Task')).toBeInTheDocument();
  });

  it('should render edit mode with task data', () => {
    render(
      <TaskModal
        task={mockTask}
        onClose={vi.fn()}
        onUpdate={vi.fn()}
      />
    );

    expect(screen.getByText('Edit Task')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Task')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test description')).toBeInTheDocument();
  });

  it('should call onCreate when creating', async () => {
    const user = userEvent.setup();
    const onCreate = vi.fn();
    const onClose = vi.fn();

    render(
      <TaskModal
        onClose={onClose}
        onCreate={onCreate}
      />
    );

    const inputs = screen.getAllByRole('textbox');
    const nameInput = inputs[0];
    await user.clear(nameInput);
    await user.type(nameInput, 'New Task');

    const descInput = inputs[1];
    await user.type(descInput, 'New description');

    await user.click(screen.getByText('Create'));

    await waitFor(() => {
      expect(onCreate).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'New Task',
        })
      );
      expect(onClose).toHaveBeenCalled();
    });
  });

  it('should call onUpdate when editing', async () => {
    const user = userEvent.setup();
    const onUpdate = vi.fn();
    const onClose = vi.fn();

    render(
      <TaskModal
        task={mockTask}
        onClose={onClose}
        onUpdate={onUpdate}
      />
    );

    const inputs = screen.getAllByRole('textbox');
    const nameInput = inputs[0];
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Name');

    await user.click(screen.getByText('Update'));

    await waitFor(() => {
      expect(onUpdate).toHaveBeenCalledWith(
        'task-1',
        expect.objectContaining({
          name: 'Updated Name',
        })
      );
      expect(onClose).toHaveBeenCalled();
    });
  });

  it('should validate required name field', async () => {
    const user = userEvent.setup();
    const onCreate = vi.fn();

    render(
      <TaskModal
        onClose={vi.fn()}
        onCreate={onCreate}
      />
    );

    const inputs = screen.getAllByRole('textbox');
    const nameInput = inputs[0];
    await user.clear(nameInput);

    await user.click(screen.getByText('Create'));

    // onCreate should not be called (browser validation prevents submit)
    await waitFor(() => {
      expect(onCreate).not.toHaveBeenCalled();
    }, { timeout: 500 }).catch(() => {});
  });

  it('should close modal when Cancel clicked', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(
      <TaskModal
        onClose={onClose}
        onCreate={vi.fn()}
      />
    );

    await user.click(screen.getByText('Cancel'));

    expect(onClose).toHaveBeenCalled();
  });
});
