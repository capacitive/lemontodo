import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../../test/utils';
import { ArchiveView } from './ArchiveView';

describe('ArchiveView', () => {
  it('should render search input', async () => {
    render(<ArchiveView />);

    expect(screen.getByPlaceholderText(/search archive/i)).toBeInTheDocument();
  });

  it('should display archived tasks', async () => {
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });
  });

  it('should filter tasks by search query', async () => {
    const user = userEvent.setup();
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText(/search archive/i);
    await user.type(searchInput, 'Archived');

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });
  });

  it('should display no results when search has no matches', async () => {
    const user = userEvent.setup();
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText(/search archive/i);
    await user.clear(searchInput);
    await user.type(searchInput, 'nonexistent');

    await waitFor(() => {
      expect(screen.queryByText('Archived Task')).not.toBeInTheDocument();
    });
  });

  it('should show loading state', () => {
    render(<ArchiveView />);

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should display results count', async () => {
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText(/result/i)).toBeInTheDocument();
    });
  });

  it('should display both due date and closed date on cards', async () => {
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
    });

    expect(screen.getByText('Due: 2026-01-01')).toBeInTheDocument();
    expect(screen.getByText('Due: 2026-02-15')).toBeInTheDocument();
    // Closed dates are rendered via toLocaleDateString()
    expect(screen.getAllByText(/Closed:/)).toHaveLength(2);
  });

  it('should sort archived tasks by due date ascending', async () => {
    render(<ArchiveView />);

    await waitFor(() => {
      expect(screen.getByText('Archived Task')).toBeInTheDocument();
      expect(screen.getByText('Archived Task Later')).toBeInTheDocument();
    });

    // Mock data has "Archived Task Later" (due 2026-02-15) before "Archived Task" (due 2026-01-01)
    // After sorting, "Archived Task" (earlier due date) should come first
    const taskNames = Array.from(document.querySelectorAll('strong')).map((el) => el.textContent);
    const indexEarlier = taskNames.indexOf('Archived Task');
    const indexLater = taskNames.indexOf('Archived Task Later');
    expect(indexEarlier).toBeLessThan(indexLater);
  });
});
