using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Repositories;

public class ActiveTaskRepository : IActiveTaskRepository
{
    private readonly ActiveDbContext _db;

    public ActiveTaskRepository(ActiveDbContext db) => _db = db;

    public async Task<TodoTask?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Tasks.FindAsync([id], ct);

    public async Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default)
        => await _db.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(TodoTask task, CancellationToken ct = default)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TodoTask task, CancellationToken ct = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var task = await _db.Tasks.FindAsync([id], ct);
        if (task is not null)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(ct);
        }
    }
}
