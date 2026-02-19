import { describe, it, expect } from 'vitest';
import { taskApi, archiveApi } from './taskApi';

describe('taskApi', () => {
  describe('getAll', () => {
    it('should fetch all tasks', async () => {
      const tasks = await taskApi.getAll();
      expect(tasks).toHaveLength(2);
      expect(tasks[0].name).toBe('Test Task 1');
      expect(tasks[1].name).toBe('Test Task 2');
    });
  });

  describe('getById', () => {
    it('should fetch a task by id', async () => {
      const task = await taskApi.getById('test-task-1');
      expect(task.id).toBe('test-task-1');
      expect(task.name).toBe('Test Task 1');
    });

    it('should throw error for non-existent task', async () => {
      await expect(taskApi.getById('nonexistent')).rejects.toThrow();
    });
  });

  describe('create', () => {
    it('should create a new task', async () => {
      const newTask = await taskApi.create({
        name: 'New Task',
        description: 'New description',
        completionDate: '2026-04-01',
      });

      expect(newTask.name).toBe('New Task');
      expect(newTask.description).toBe('New description');
      expect(newTask.status).toBe('Open');
      expect(newTask.id).toBeDefined();
    });
  });

  describe('update', () => {
    it('should update an existing task', async () => {
      const updated = await taskApi.update('test-task-1', {
        name: 'Updated Name',
        description: 'Updated desc',
        completionDate: '2026-05-01',
      });

      expect(updated.name).toBe('Updated Name');
      expect(updated.description).toBe('Updated desc');
    });
  });

  describe('close', () => {
    it('should close an open task', async () => {
      const closed = await taskApi.close('test-task-1');
      expect(closed.status).toBe('Closed');
      expect(closed.closedAt).toBeDefined();
    });

    it('should return 409 for already closed task', async () => {
      await expect(taskApi.close('test-task-2')).rejects.toThrow('409');
    });
  });

  describe('reopen', () => {
    it('should reopen a closed task', async () => {
      const reopened = await taskApi.reopen('test-task-2');
      expect(reopened.status).toBe('Reopened');
      expect(reopened.reopenedAt).toBeDefined();
    });

    it('should return 409 for non-closed task', async () => {
      await expect(taskApi.reopen('test-task-1')).rejects.toThrow('409');
    });
  });
});

describe('archiveApi', () => {
  describe('search', () => {
    it('should search archived tasks', async () => {
      const result = await archiveApi.search('', 1, 20);
      expect(result.items).toHaveLength(2);
      expect(result.items[0].name).toBe('Archived Task Later');
      expect(result.totalCount).toBe(2);
    });

    it('should filter by search query', async () => {
      const result = await archiveApi.search('Archived', 1, 20);
      expect(result.items).toHaveLength(2);

      const noResults = await archiveApi.search('nonexistent', 1, 20);
      expect(noResults.items).toHaveLength(0);
    });
  });

  describe('restore', () => {
    it('should restore an archived task', async () => {
      const restored = await archiveApi.restore('archived-task-1');
      expect(restored.status).toBe('Reopened');
      expect(restored.reopenedAt).toBeDefined();
    });
  });
});
