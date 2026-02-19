using LemonTodo.Application.DTOs;

namespace LemonTodo.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(string userId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse?> GetByIdAsync(string userId, string id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(string userId, CancellationToken ct = default);
    Task<TaskResponse> UpdateAsync(string userId, string id, UpdateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> StartAsync(string userId, string id, CancellationToken ct = default);
    Task<TaskResponse> CloseAsync(string userId, string id, CancellationToken ct = default);
    Task<TaskResponse> ReopenAsync(string userId, string id, CancellationToken ct = default);
}
