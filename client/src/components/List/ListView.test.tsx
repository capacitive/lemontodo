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

  it('should show Reopen button for closed and archived tasks', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
      expect(screen.getByText('Archived Task Later')).toBeInTheDocument();
    });

    const reopenButtons = screen.getAllByText('Reopen');
    expect(reopenButtons).toHaveLength(3);
  });

  it('should display both due date and closed date for recently closed tasks', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    // Recently closed section should show "Due:" and "Closed:" labels
    expect(screen.getByText('Due: 2026-01-01')).toBeInTheDocument();
    expect(screen.getByText('Due: 2026-02-15')).toBeInTheDocument();
    expect(screen.getAllByText(/Closed:/).length).toBeGreaterThanOrEqual(3);
  });

  it('should sort recently closed tasks by due date ascending', async () => {
    render(<ListView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
      expect(screen.getByText('Archived Task Later')).toBeInTheDocument();
      expect(screen.getByText('Test Task 2')).toBeInTheDocument();
    });

    // In recently closed section, items with line-through text are the task names
    const closedNames = Array.from(document.querySelectorAll('span'))
      .filter((el) => el.style.textDecoration === 'line-through')
      .map((el) => el.textContent);

    // Due dates: Archived Task (2026-01-01), Archived Task Later (2026-02-15), Test Task 2 (2026-03-15)
    const i1 = closedNames.indexOf('Archived Task');
    const i2 = closedNames.indexOf('Archived Task Later');
    const i3 = closedNames.indexOf('Test Task 2');
    expect(i1).toBeLessThan(i2);
    expect(i2).toBeLessThan(i3);
  });
});
