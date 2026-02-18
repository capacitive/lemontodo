import type { CreateTaskRequest, PagedResponse, TaskResponse, UpdateTaskRequest } from '../types';
import { api } from './client';

export const taskApi = {
  getAll: () => api.get<TaskResponse[]>('/tasks'),
  getById: (id: string) => api.get<TaskResponse>(`/tasks/${id}`),
  create: (req: CreateTaskRequest) => api.post<TaskResponse>('/tasks', req),
  update: (id: string, req: UpdateTaskRequest) => api.put<TaskResponse>(`/tasks/${id}`, req),
  close: (id: string) => api.patch<TaskResponse>(`/tasks/${id}/close`),
  reopen: (id: string) => api.patch<TaskResponse>(`/tasks/${id}/reopen`),
};

export const archiveApi = {
  search: (q?: string, page = 1, pageSize = 20) =>
    api.get<PagedResponse<TaskResponse>>(`/archive?q=${q ?? ''}&page=${page}&pageSize=${pageSize}`),
  getById: (id: string) => api.get<TaskResponse>(`/archive/${id}`),
  restore: (id: string) => api.patch<TaskResponse>(`/archive/${id}/restore`),
  delete: (id: string) => api.delete(`/archive/${id}`),
  purgeAll: () => api.delete('/archive/purge'),
};
