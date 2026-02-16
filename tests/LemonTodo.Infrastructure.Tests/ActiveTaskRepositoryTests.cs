using FluentAssertions;
using LemonTodo.Domain;
using LemonTodo.Infrastructure.Data;
using LemonTodo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Tests;

public class ActiveTaskRepositoryTests : IDisposable
{
    private readonly ActiveDbContext _db;
    private readonly ActiveTaskRepository _repo;

    public ActiveTaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ActiveDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ActiveDbContext(options);
        _repo = new ActiveTaskRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAndGetById_RoundTrip()
    {
        var task = TodoTask.Create("id1", "Test", "Desc", new DateOnly(2026, 3, 1));
        await _repo.AddAsync(task);

        var result = await _repo.GetByIdAsync("id1");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync("nope");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsOrderedByCreatedAtDescending()
    {
        var t1 = TodoTask.Create("id1", "First", null, new DateOnly(2026, 3, 1), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var t2 = TodoTask.Create("id2", "Second", null, new DateOnly(2026, 3, 1), new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        await _repo.AddAsync(t1);
        await _repo.AddAsync(t2);

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(2);
        all[0].Id.Should().Be("id2");
        all[1].Id.Should().Be("id1");
    }

    [Fact]
    public async Task Update_ModifiesTask()
    {
        var task = TodoTask.Create("id1", "Old", null, new DateOnly(2026, 3, 1));
        await _repo.AddAsync(task);

        task.Update("New", "Updated", new DateOnly(2026, 4, 1));
        await _repo.UpdateAsync(task);

        var result = await _repo.GetByIdAsync("id1");
        result!.Name.Should().Be("New");
        result.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        await _repo.AddAsync(task);

        await _repo.DeleteAsync("id1");

        var result = await _repo.GetByIdAsync("id1");
        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistent_DoesNotThrow()
    {
        var act = () => _repo.DeleteAsync("nope");
        await act.Should().NotThrowAsync();
    }
}
