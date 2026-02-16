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

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        var task = TodoTask.Create(_idGen.NewId(), request.Name, request.Description, request.CompletionDate);
        await _repo.AddAsync(task, ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var task = await _repo.GetByIdAsync(id, ct);
        return task?.ToResponse();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tasks = await _repo.GetAllAsync(ct);
        return tasks.Select(t => t.ToResponse()).ToList();
    }

    public async Task<TaskResponse> UpdateAsync(string id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Task '{id}' not found.");

        task.Update(request.Name, request.Description, request.CompletionDate);
        await _repo.UpdateAsync(task, ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse> CloseAsync(string id, CancellationToken ct = default)
    {
        var task = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Task '{id}' not found.");

        task.Close();
        await _repo.UpdateAsync(task, ct);
        await _channel.PublishAsync(new TaskClosedEvent(task.Id, DateTime.UtcNow), ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse> ReopenAsync(string id, CancellationToken ct = default)
    {
        var task = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Task '{id}' not found.");

        task.Reopen();
        await _repo.UpdateAsync(task, ct);
        return task.ToResponse();
    }
}
