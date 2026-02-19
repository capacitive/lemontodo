using LemonTodo.Application.DTOs;

namespace LemonTodo.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(CancellationToken ct = default);
    Task<TaskResponse> UpdateAsync(string id, UpdateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> StartAsync(string id, CancellationToken ct = default);
    Task<TaskResponse> CloseAsync(string id, CancellationToken ct = default);
    Task<TaskResponse> ReopenAsync(string id, CancellationToken ct = default);
}
