import { describe, it, expect, vi } from 'vitest';
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
});
