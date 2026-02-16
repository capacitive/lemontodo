using FluentAssertions;
using LemonTodo.Domain;
using LemonTodo.Infrastructure.Data;
using LemonTodo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Tests;

public class ArchiveTaskRepositoryTests : IDisposable
{
    private readonly ArchiveDbContext _db;
    private readonly ArchiveTaskRepository _repo;

    public ArchiveTaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ArchiveDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new ArchiveDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _repo = new ArchiveTaskRepository(_db);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    private TodoTask CreateClosedTask(string id, string name, string? desc = null, DateTime? closedAt = null)
    {
        var task = TodoTask.Create(id, name, desc, new DateOnly(2026, 3, 1));
        task.Close(closedAt);
        return task;
    }

    [Fact]
    public async Task AddAndGetById_RoundTrip()
    {
        var task = CreateClosedTask("id1", "Archived task");
        await _repo.AddAsync(task);

        var result = await _repo.GetByIdAsync("id1");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Archived task");
        result.Status.Should().Be(TodoTaskStatus.Closed);
    }

    [Fact]
    public async Task Search_WithNoQuery_ReturnsAll()
    {
        await _repo.AddAsync(CreateClosedTask("id1", "A", closedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        await _repo.AddAsync(CreateClosedTask("id2", "B", closedAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)));

        var (items, count) = await _repo.SearchAsync(null, 1, 10);

        count.Should().Be(2);
        items.Should().HaveCount(2);
        items[0].Id.Should().Be("id2"); // most recently closed first
    }

    [Fact]
    public async Task Search_WithQuery_FiltersbyNameAndDescription()
    {
        await _repo.AddAsync(CreateClosedTask("id1", "Buy groceries", "Milk and bread"));
        await _repo.AddAsync(CreateClosedTask("id2", "Fix bugs", "Debug the login"));

        var (items, count) = await _repo.SearchAsync("grocer", 1, 10);

        count.Should().Be(1);
        items[0].Id.Should().Be("id1");
    }

    [Fact]
    public async Task Search_WithQuery_MatchesDescription()
    {
        await _repo.AddAsync(CreateClosedTask("id1", "Task A", "contains keyword here"));
        await _repo.AddAsync(CreateClosedTask("id2", "Task B", "nothing special"));

        var (items, count) = await _repo.SearchAsync("keyword", 1, 10);

        count.Should().Be(1);
        items[0].Id.Should().Be("id1");
    }

    [Fact]
    public async Task Search_Pagination_Works()
    {
        for (int i = 1; i <= 5; i++)
            await _repo.AddAsync(CreateClosedTask($"id{i}", $"Task {i}", closedAt: new DateTime(2026, 1, i, 0, 0, 0, DateTimeKind.Utc)));

        var (page1, total1) = await _repo.SearchAsync(null, 1, 2);
        var (page2, total2) = await _repo.SearchAsync(null, 2, 2);

        total1.Should().Be(5);
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        await _repo.AddAsync(CreateClosedTask("id1", "Task"));
        await _repo.DeleteAsync("id1");

        var result = await _repo.GetByIdAsync("id1");
        result.Should().BeNull();
    }
}
