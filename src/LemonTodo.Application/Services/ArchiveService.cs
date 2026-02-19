using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Mapping;
using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Application.Services;

public class ArchiveService : IArchiveService
{
    private readonly IArchiveTaskRepository _archiveRepo;
    private readonly IActiveTaskRepository _activeRepo;

    public ArchiveService(IArchiveTaskRepository archiveRepo, IActiveTaskRepository activeRepo)
    {
        _archiveRepo = archiveRepo;
        _activeRepo = activeRepo;
    }

    public async Task<TaskResponse?> GetByIdAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await _archiveRepo.GetByIdAsync(id, ct);
        if (task is null || task.UserId != userId) return null;
        return task.ToResponse();
    }

    public async Task<PagedResponse<TaskResponse>> SearchAsync(string userId, string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _archiveRepo.SearchAsync(query, page, pageSize, ct);
        var filtered = items.Where(t => t.UserId == userId).ToList();
        return new PagedResponse<TaskResponse>(
            filtered.Select(t => t.ToResponse()).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<TaskResponse> RestoreAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await _archiveRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Archived task '{id}' not found.");

        if (task.UserId != userId)
            throw new KeyNotFoundException($"Archived task '{id}' not found.");

        task.Reopen();
        await _activeRepo.AddAsync(task, ct);
        await _archiveRepo.DeleteAsync(id, ct);
        return task.ToResponse();
    }

    public async Task DeleteAsync(string userId, string id, CancellationToken ct = default)
    {
        var task = await _archiveRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Archived task '{id}' not found.");

        if (task.UserId != userId)
            throw new KeyNotFoundException($"Archived task '{id}' not found.");

        await _archiveRepo.DeleteAsync(id, ct);
    }

    public async Task PurgeAllAsync(string userId, CancellationToken ct = default)
    {
        await _archiveRepo.PurgeAllAsync(ct);
    }
}
