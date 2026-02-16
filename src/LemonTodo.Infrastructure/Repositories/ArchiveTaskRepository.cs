using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Repositories;

public class ArchiveTaskRepository : IArchiveTaskRepository
{
    private readonly ArchiveDbContext _db;

    public ArchiveTaskRepository(ArchiveDbContext db) => _db = db;

    public async Task<TodoTask?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Tasks.FindAsync([id], ct);

    public async Task<(IReadOnlyList<TodoTask> Items, int TotalCount)> SearchAsync(
        string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Tasks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            q = q.Where(t => t.Name.ToLower().Contains(term) ||
                             (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(t => t.ClosedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(TodoTask task, CancellationToken ct = default)
    {
        _db.Tasks.Add(task);
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
