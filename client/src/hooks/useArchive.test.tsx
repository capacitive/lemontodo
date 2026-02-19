import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useArchiveSearch, useRestoreTask } from './useArchive';

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}

describe('useArchiveSearch', () => {
  it('should search archived tasks', async () => {
    const { result } = renderHook(() => useArchiveSearch('', 1, 20), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data?.items).toHaveLength(2);
    expect(result.current.data?.items[0].name).toBe('Archived Task Later');
    expect(result.current.data?.totalCount).toBe(2);
  });

  it('should filter by search query', async () => {
    const { result } = renderHook(() => useArchiveSearch('Archived', 1, 20), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data?.items).toHaveLength(2);
  });

  it('should return no results for non-matching query', async () => {
    const { result } = renderHook(() => useArchiveSearch('nonexistent', 1, 20), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data?.items).toHaveLength(0);
  });
});

describe('useRestoreTask', () => {
  it('should restore a task', async () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useRestoreTask(), { wrapper });

    result.current.mutate('archived-task-1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
