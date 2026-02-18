namespace LemonTodo.Domain.Interfaces;

public interface IArchiveTaskRepository
{
    Task<TodoTask?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<TodoTask> Items, int TotalCount)> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(TodoTask task, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task PurgeAllAsync(CancellationToken ct = default);
}
