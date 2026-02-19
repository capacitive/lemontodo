import { http, HttpResponse } from 'msw';
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest, PagedResponse } from '../../types';

const BASE_URL = 'http://localhost:5175/api';

// Mock data
let mockTasks: TaskResponse[] = [
  {
    id: 'test-task-1',
    name: 'Test Task 1',
    description: 'Description 1',
    completionDate: '2026-03-01',
    status: 'Open',
    createdAt: '2026-02-16T10:00:00Z',
    closedAt: null,
    reopenedAt: null,
  },
  {
    id: 'test-task-2',
    name: 'Test Task 2',
    description: null,
    completionDate: '2026-03-15',
    status: 'Closed',
    createdAt: '2026-02-15T09:00:00Z',
    closedAt: '2026-02-16T12:00:00Z',
    reopenedAt: null,
  },
];

let mockArchive: TaskResponse[] = [
  {
    id: 'archived-task-1',
    name: 'Archived Task',
    description: 'Old task',
    completionDate: '2026-01-01',
    status: 'Closed',
    createdAt: '2026-01-01T10:00:00Z',
    closedAt: '2026-01-05T15:00:00Z',
    reopenedAt: null,
  },
];

export const handlers = [
  // GET /api/tasks - List all tasks
  http.get(`${BASE_URL}/tasks`, () => {
    return HttpResponse.json(mockTasks);
  }),

  // GET /api/tasks/:id - Get task by ID
  http.get(`${BASE_URL}/tasks/:id`, ({ params }) => {
    const task = mockTasks.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    return HttpResponse.json(task);
  }),

  // POST /api/tasks - Create task
  http.post(`${BASE_URL}/tasks`, async ({ request }) => {
    const body = (await request.json()) as CreateTaskRequest;
    const newTask: TaskResponse = {
      id: `task-${Date.now()}`,
      name: body.name,
      description: body.description || null,
      completionDate: body.completionDate,
      status: 'Open',
      createdAt: new Date().toISOString(),
      closedAt: null,
      reopenedAt: null,
    };
    mockTasks.push(newTask);
    return HttpResponse.json(newTask, { status: 201 });
  }),

  // PUT /api/tasks/:id - Update task
  http.put(`${BASE_URL}/tasks/:id`, async ({ params, request }) => {
    const task = mockTasks.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    const body = (await request.json()) as UpdateTaskRequest;
    Object.assign(task, {
      name: body.name,
      description: body.description,
      completionDate: body.completionDate,
    });
    return HttpResponse.json(task);
  }),

  // PATCH /api/tasks/:id/close - Close task
  http.patch(`${BASE_URL}/tasks/:id/close`, ({ params }) => {
    const task = mockTasks.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    if (task.status === 'Closed') {
      return new HttpResponse(JSON.stringify({ error: 'Task is already closed' }), { status: 409 });
    }
    task.status = 'Closed';
    task.closedAt = new Date().toISOString();
    return HttpResponse.json(task);
  }),

  // PATCH /api/tasks/:id/reopen - Reopen task
  http.patch(`${BASE_URL}/tasks/:id/reopen`, ({ params }) => {
    const task = mockTasks.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    if (task.status !== 'Closed') {
      return new HttpResponse(JSON.stringify({ error: 'Task is not closed' }), { status: 409 });
    }
    task.status = 'Reopened';
    task.reopenedAt = new Date().toISOString();
    return HttpResponse.json(task);
  }),

  // GET /api/archive - Search archived tasks
  http.get(`${BASE_URL}/archive`, ({ request }) => {
    const url = new URL(request.url);
    const q = url.searchParams.get('q') || '';
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    let filtered = mockArchive;
    if (q) {
      filtered = mockArchive.filter((t) =>
        t.name.toLowerCase().includes(q.toLowerCase())
      );
    }

    const start = (page - 1) * pageSize;
    const items = filtered.slice(start, start + pageSize);

    const response: PagedResponse<TaskResponse> = {
      items,
      page,
      pageSize,
      totalCount: filtered.length,
    };

    return HttpResponse.json(response);
  }),

  // GET /api/archive/:id - Get archived task by ID
  http.get(`${BASE_URL}/archive/:id`, ({ params }) => {
    const task = mockArchive.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    return HttpResponse.json(task);
  }),

  // PATCH /api/archive/:id/restore - Restore archived task
  http.patch(`${BASE_URL}/archive/:id/restore`, ({ params }) => {
    const task = mockArchive.find((t) => t.id === params.id);
    if (!task) {
      return new HttpResponse(null, { status: 404 });
    }
    task.status = 'Reopened';
    task.reopenedAt = new Date().toISOString();
    mockTasks.push(task);
    mockArchive = mockArchive.filter((t) => t.id !== params.id);
    return HttpResponse.json(task);
  }),
];

// Helper to reset mock data between tests
export function resetMockData() {
  mockTasks = [
    {
      id: 'test-task-1',
      name: 'Test Task 1',
      description: 'Description 1',
      completionDate: '2026-03-01',
      status: 'Open',
      createdAt: '2026-02-16T10:00:00Z',
      closedAt: null,
      reopenedAt: null,
    },
    {
      id: 'test-task-2',
      name: 'Test Task 2',
      description: null,
      completionDate: '2026-03-15',
      status: 'Closed',
      createdAt: '2026-02-15T09:00:00Z',
      closedAt: '2026-02-16T12:00:00Z',
      reopenedAt: null,
    },
  ];

  mockArchive = [
    {
      id: 'archived-task-1',
      name: 'Archived Task',
      description: 'Old task',
      completionDate: '2026-01-01',
      status: 'Closed',
      createdAt: '2026-01-01T10:00:00Z',
      closedAt: '2026-01-05T15:00:00Z',
      reopenedAt: null,
    },
  ];
}
