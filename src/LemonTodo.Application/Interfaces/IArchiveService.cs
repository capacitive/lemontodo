using LemonTodo.Application.DTOs;

namespace LemonTodo.Application.Interfaces;

public interface IArchiveService
{
    Task<TaskResponse?> GetByIdAsync(string userId, string id, CancellationToken ct = default);
    Task<PagedResponse<TaskResponse>> SearchAsync(string userId, string? query, int page, int pageSize, CancellationToken ct = default);
    Task<TaskResponse> RestoreAsync(string userId, string id, CancellationToken ct = default);
    Task DeleteAsync(string userId, string id, CancellationToken ct = default);
    Task PurgeAllAsync(string userId, CancellationToken ct = default);
}
