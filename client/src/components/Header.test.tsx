import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '../test/utils';
import { Header } from './Layout/Header';

describe('Header', () => {
  it('should render app title', () => {
    render(<Header view="board" onViewChange={vi.fn()} />);

    expect(screen.getByText('LemonTodo')).toBeInTheDocument();
  });

  it('should render all view buttons', () => {
    render(<Header view="board" onViewChange={vi.fn()} />);

    expect(screen.getByText('board')).toBeInTheDocument();
    expect(screen.getByText('list')).toBeInTheDocument();
    expect(screen.getByText('archive')).toBeInTheDocument();
  });

  it('should highlight active view', () => {
    const { rerender } = render(<Header view="board" onViewChange={vi.fn()} />);

    const boardButton = screen.getByText('board');
    expect(boardButton).toHaveStyle({ background: '#fbbf24' });

    rerender(<Header view="list" onViewChange={vi.fn()} />);
    const listButton = screen.getByText('list');
    expect(listButton).toHaveStyle({ background: '#fbbf24' });

    rerender(<Header view="archive" onViewChange={vi.fn()} />);
    const archiveButton = screen.getByText('archive');
    expect(archiveButton).toHaveStyle({ background: '#fbbf24' });
  });

  it('should call onViewChange when view button clicked', async () => {
    const user = userEvent.setup();
    const onViewChange = vi.fn();

    render(<Header view="board" onViewChange={onViewChange} />);

    await user.click(screen.getByText('list'));
    expect(onViewChange).toHaveBeenCalledWith('list');

    await user.click(screen.getByText('archive'));
    expect(onViewChange).toHaveBeenCalledWith('archive');

    await user.click(screen.getByText('board'));
    expect(onViewChange).toHaveBeenCalledWith('board');
  });

  it('should call onViewChange when clicking active view', async () => {
    const user = userEvent.setup();
    const onViewChange = vi.fn();

    render(<Header view="board" onViewChange={onViewChange} />);

    await user.click(screen.getByText('board'));

    expect(onViewChange).toHaveBeenCalledWith('board');
  });
});
