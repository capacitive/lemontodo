using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Mapping;
using LemonTodo.Domain;
using LemonTodo.Domain.Events;
using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Application.Services;

public class TaskService : ITaskService
{
    private readonly IActiveTaskRepository _repo;
    private readonly IIdGenerator _idGen;
    private readonly ITaskEventChannel _channel;

    public TaskService(IActiveTaskRepository repo, IIdGenerator idGen, ITaskEventChannel channel)
    {
        _repo = repo;
        _idGen = idGen;
        _channel = channel;
    }

    public async Task<TaskResponse> CreateAsync(string userId, CreateTaskRequest request, CancellationToken ct = default)
    {
        var task = TodoTask.Create(_idGen.NewId(), request.Name, request.Description, request.CompletionDate, userId: userId);
        await _repo.AddAsync(task, ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse?> GetByIdAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await _repo.GetByIdAsync(id, ct);
        if (task is null || task.UserId != userId) return null;
        return task.ToResponse();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(string userId, CancellationToken ct = default)
    {
        var tasks = await _repo.GetAllAsync(ct);
        return tasks.Where(t => t.UserId == userId).Select(t => t.ToResponse()).ToList();
    }

    public async Task<TaskResponse> UpdateAsync(string userId, string id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await GetOwnedTaskAsync(userId, id, ct);
        task.Update(request.Name, request.Description, request.CompletionDate);
        await _repo.UpdateAsync(task, ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse> CloseAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await GetOwnedTaskAsync(userId, id, ct);
        task.Close();
        await _repo.UpdateAsync(task, ct);
        await _channel.PublishAsync(new TaskClosedEvent(task.Id, DateTime.UtcNow), ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse> ReopenAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await GetOwnedTaskAsync(userId, id, ct);
        task.Reopen();
        await _repo.UpdateAsync(task, ct);
        return task.ToResponse();
    }

    private async Task<TodoTask> GetOwnedTaskAsync(string userId, string id, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Task '{id}' not found.");

        if (task.UserId != userId)
            throw new KeyNotFoundException($"Task '{id}' not found.");

        return task;
    }
}
