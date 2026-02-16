namespace LemonTodo.Domain.Interfaces;

public interface IActiveTaskRepository
{
    Task<TodoTask?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TodoTask task, CancellationToken ct = default);
    Task UpdateAsync(TodoTask task, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
