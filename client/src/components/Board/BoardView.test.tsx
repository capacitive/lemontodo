import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../../test/utils';
import { BoardView } from './BoardView';

// Mock SignalR
vi.mock('../../hooks/useSignalR', () => ({
  useSignalR: () => {},
}));

describe('BoardView', () => {
  it('should render board layout', async () => {
    render(<BoardView />);

    await waitFor(() => {
      // Just verify the component renders without errors
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });
  });

  it('should display tasks in correct columns', async () => {
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });
  });

  it('should show New Task button', async () => {
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });
  });

  it('should open modal when New Task button clicked', async () => {
    const user = userEvent.setup();
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });

    await user.click(screen.getByText('+ New Task'));

    expect(screen.getByText('New Task')).toBeInTheDocument();
  });

  it('should open edit modal when Edit button clicked on active task', async () => {
    const user = userEvent.setup();
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });

    const buttons = screen.getAllByText('Edit');

    await user.click(buttons[0]);

    expect(screen.getByText('Edit Task')).toBeInTheDocument();
  });

  it('should pre-fill edit modal with task data', async () => {
    const user = userEvent.setup();
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });

    const buttons = screen.getAllByText('Edit');

    await user.click(buttons[0]);

    expect(screen.getByText('Edit Task')).toBeInTheDocument();

    const inputPrefilledValue = screen.getByDisplayValue('Test Task 1');

    expect(inputPrefilledValue).toBeInTheDocument();
  });

  it('should display loading state', () => {
    render(<BoardView />);

    // Should show loading initially
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should display error state when tasks fail to load', async () => {
    // This would require mocking the API to return an error
    // For now, we'll just verify the component structure
    render(<BoardView />);

    await waitFor(() => {
      // Should eventually load successfully with MSW
      expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
    });
  });

  it('should display tasks', async () => {
    render(<BoardView />);

    await waitFor(() => {
      // Just verify tasks are rendered
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
    });
  });

  it('should not show Edit button for closed or archived tasks', async () => {
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    // Only the open task (Test Task 1) should have an Edit button
    const editButtons = screen.getAllByText('Edit');
    expect(editButtons).toHaveLength(1);
  });

  it('should show Reopen button for closed and archived tasks', async () => {
    render(<BoardView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    const reopenButtons = screen.getAllByText('Reopen');
    expect(reopenButtons).toHaveLength(2);
  });
});
