import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { CreateTaskRequest, UpdateTaskRequest } from '../types';
import { taskApi } from '../api/taskApi';

export function useTasks() {
  return useQuery({
    queryKey: ['tasks'],
    queryFn: taskApi.getAll,
  });
}

export function useCreateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateTaskRequest) => taskApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}

export function useUpdateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateTaskRequest }) => taskApi.update(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}

export function useCloseTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => taskApi.close(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['archive'] });
    },
  });
}

export function useReopenTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => taskApi.reopen(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}
