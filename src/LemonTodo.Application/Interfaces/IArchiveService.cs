using LemonTodo.Application.DTOs;

namespace LemonTodo.Application.Interfaces;

public interface IArchiveService
{
    Task<TaskResponse?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResponse<TaskResponse>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task<TaskResponse> RestoreAsync(string id, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task PurgeAllAsync(CancellationToken ct = default);
}
