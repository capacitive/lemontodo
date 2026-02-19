using FluentAssertions;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Services;
using LemonTodo.Domain;
using LemonTodo.Domain.Events;
using LemonTodo.Domain.Interfaces;
using NSubstitute;

namespace LemonTodo.Application.Tests;

public class TaskServiceTests
{
    private const string UserId = "test-user-id";
    private readonly IActiveTaskRepository _repo = Substitute.For<IActiveTaskRepository>();
    private readonly IIdGenerator _idGen = Substitute.For<IIdGenerator>();
    private readonly ITaskEventChannel _channel = Substitute.For<ITaskEventChannel>();
    private readonly TaskService _svc;

    public TaskServiceTests()
    {
        _idGen.NewId().Returns("test-id-123");
        _svc = new TaskService(_repo, _idGen, _channel);
    }

    [Fact]
    public async Task Create_ReturnsTaskWithGeneratedId()
    {
        var request = new CreateTaskRequest("Buy groceries", "Milk", new DateOnly(2026, 3, 1));

        var result = await _svc.CreateAsync(UserId, request);

        result.Id.Should().Be("test-id-123");
        result.Name.Should().Be("Buy groceries");
        result.Status.Should().Be("Open");
        await _repo.Received(1).AddAsync(Arg.Is<TodoTask>(t => t.Id == "test-id-123" && t.UserId == UserId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsResponse()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), userId: UserId);
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.GetByIdAsync(UserId, "id1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("id1");
    }

    [Fact]
    public async Task GetById_WhenNotExists_ReturnsNull()
    {
        _repo.GetByIdAsync("nope", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        var result = await _svc.GetByIdAsync(UserId, "nope");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_WhenDifferentUser_ReturnsNull()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), userId: "other-user");
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.GetByIdAsync(UserId, "id1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        var tasks = new List<TodoTask>
        {
            TodoTask.Create("id1", "A", null, new DateOnly(2026, 3, 1), userId: UserId),
            TodoTask.Create("id2", "B", null, new DateOnly(2026, 3, 1), userId: UserId),
            TodoTask.Create("id3", "C", null, new DateOnly(2026, 3, 1), userId: "other-user")
        };
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        var result = await _svc.GetAllAsync(UserId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesAndReturns()
    {
        var task = TodoTask.Create("id1", "Old", null, new DateOnly(2026, 3, 1), userId: UserId);
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.UpdateAsync(UserId, "id1", new UpdateTaskRequest("New", "Desc", new DateOnly(2026, 4, 1)));

        result.Name.Should().Be("New");
        result.Description.Should().Be("Desc");
        await _repo.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenNotExists_ThrowsKeyNotFound()
    {
        _repo.GetByIdAsync("nope", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        var act = () => _svc.UpdateAsync(UserId, "nope", new UpdateTaskRequest("Name", null, new DateOnly(2026, 3, 1)));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Start_SetsInProgressStatus()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), userId: UserId);
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.StartAsync(UserId, "id1");

        result.Status.Should().Be("InProgress");
        await _repo.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Close_SetsClosedAndPublishesEvent()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), userId: UserId);
        task.Start();
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.CloseAsync(UserId, "id1");

        result.Status.Should().Be("Closed");
        await _channel.Received(1).PublishAsync(Arg.Is<TaskClosedEvent>(e => e.TaskId == "id1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reopen_SetsReopenedStatus()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), userId: UserId);
        task.Start();
        task.Close();
        _repo.GetByIdAsync("id1", Arg.Any<CancellationToken>()).Returns(task);

        var result = await _svc.ReopenAsync(UserId, "id1");

        result.Status.Should().Be("Reopened");
        await _repo.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }
}
