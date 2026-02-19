export type TodoTaskStatus = 'Open' | 'InProgress' | 'Closed' | 'Reopened';

export interface TaskResponse {
  id: string;
  name: string;
  description: string | null;
  completionDate: string;
  status: TodoTaskStatus;
  createdAt: string;
  startedAt: string | null;
  closedAt: string | null;
  reopenedAt: string | null;
}

export interface CreateTaskRequest {
  name: string;
  description: string | null;
  completionDate: string;
}

export interface UpdateTaskRequest {
  name: string;
  description: string | null;
  completionDate: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type ViewMode = 'board' | 'list' | 'archive';
