import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../../test/utils';
import { ListView } from './ListView';

// Mock SignalR
vi.mock('../../hooks/useSignalR', () => ({
  useSignalR: () => {},
}));

describe('ListView', () => {
  it('should render list layout', async () => {
    render(<ListView />);

    await waitFor(() => {
      // Just verify component renders
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });
  });

  it('should display tasks in correct sections', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });
  });

  it('should show New Task button', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });
  });

  it('should open modal when New Task button clicked', async () => {
    const user = userEvent.setup();
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('+ New Task')).toBeInTheDocument();
    });

    await user.click(screen.getByText('+ New Task'));

    expect(screen.getByText('New Task')).toBeInTheDocument();
  });

  it('should open edit modal when Edit button clicked on active task', async () => {
    const user = userEvent.setup();
    render(<ListView />);

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
      render(<ListView />);
  
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
    render(<ListView />);

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should display all tasks', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });
  });
});
