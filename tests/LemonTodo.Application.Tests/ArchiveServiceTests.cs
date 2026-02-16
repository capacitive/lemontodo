using FluentAssertions;
using LemonTodo.Application.Services;
using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;
using NSubstitute;

namespace LemonTodo.Application.Tests;

public class ArchiveServiceTests
{
    private readonly IArchiveTaskRepository _archiveRepo = Substitute.For<IArchiveTaskRepository>();
    private readonly IActiveTaskRepository _activeRepo = Substitute.For<IActiveTaskRepository>();
    private readonly ArchiveService _svc;

    public ArchiveServiceTests()
    {
        _svc = new ArchiveService(_archiveRepo, _activeRepo);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsResponse()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();
        _archiveRepo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.GetByIdAsync("id1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("id1");
    }

    [Fact]
    public async Task Search_ReturnsPaged()
    {
        var tasks = new List<TodoTask>
        {
            TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1))
        };
        tasks[0].Close();
        _archiveRepo.SearchAsync("task", 1, 10, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<TodoTask>)tasks, 1));

        var result = await _svc.SearchAsync("task", 1, 10);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Restore_MovesFromArchiveToActive()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();
        _archiveRepo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.RestoreAsync("id1");

        result.Status.Should().Be("Reopened");
        await _activeRepo.Received(1).AddAsync(task, Arg.Any<CancellationToken>());
        await _archiveRepo.Received(1).DeleteAsync("id1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Restore_WhenNotFound_Throws()
    {
        _archiveRepo.GetByIdAsync("nope", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        var act = () => _svc.RestoreAsync("nope");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
