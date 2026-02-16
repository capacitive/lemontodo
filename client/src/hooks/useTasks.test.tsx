import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useTasks, useCreateTask, useUpdateTask, useCloseTask, useReopenTask } from './useTasks';

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

describe('useTasks', () => {
  it('should fetch tasks on mount', async () => {
    const { result } = renderHook(() => useTasks(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data).toHaveLength(2);
    expect(result.current.data?.[0].name).toBe('Test Task 1');
  });
});

describe('useCreateTask', () => {
  it('should create a new task', async () => {
    const wrapper = createWrapper();
    const { result: createResult } = renderHook(() => useCreateTask(), { wrapper });
    const { result: tasksResult } = renderHook(() => useTasks(), { wrapper });

    await waitFor(() => expect(tasksResult.current.isLoading).toBe(false));

    createResult.current.mutate({
      name: 'New Task',
      description: 'Desc',
      completionDate: '2026-04-01',
    });

    await waitFor(() => expect(createResult.current.isSuccess).toBe(true));
  });
});

describe('useUpdateTask', () => {
  it('should update a task', async () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useUpdateTask(), { wrapper });

    result.current.mutate({
      id: 'test-task-1',
      req: {
        name: 'Updated Name',
        description: 'Updated',
        completionDate: '2026-05-01',
      },
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe('useCloseTask', () => {
  it('should close a task', async () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useCloseTask(), { wrapper });

    result.current.mutate('test-task-1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe('useReopenTask', () => {
  it('should reopen a task', async () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useReopenTask(), { wrapper });

    result.current.mutate('test-task-2');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
