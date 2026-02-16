using FluentAssertions;
using LemonTodo.Domain;
using LemonTodo.Domain.Exceptions;

namespace LemonTodo.Domain.Tests;

public class TodoTaskCreationTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsOpenTask()
    {
        var task = TodoTask.Create("abc123", "Buy groceries", "Milk and eggs", new DateOnly(2026, 3, 1));

        task.Id.Should().Be("abc123");
        task.Name.Should().Be("Buy groceries");
        task.Description.Should().Be("Milk and eggs");
        task.CompletionDate.Should().Be(new DateOnly(2026, 3, 1));
        task.Status.Should().Be(TodoTaskStatus.Open);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        task.ClosedAt.Should().BeNull();
        task.ReopenedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullDescription_Succeeds()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithExplicitCreatedAt_UsesProvidedValue()
    {
        var ts = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1), ts);
        task.CreatedAt.Should().Be(ts);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidId_Throws(string? id)
    {
        var act = () => TodoTask.Create(id!, "Name", null, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        var act = () => TodoTask.Create("id1", name!, null, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNameOver200Chars_Throws()
    {
        var longName = new string('x', 201);
        var act = () => TodoTask.Create("id1", longName, null, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>().WithMessage("*200*");
    }

    [Fact]
    public void Create_WithDescriptionOver2000Chars_Throws()
    {
        var longDesc = new string('x', 2001);
        var act = () => TodoTask.Create("id1", "Name", longDesc, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>().WithMessage("*2000*");
    }
}

public class TodoTaskCloseTests
{
    [Fact]
    public void Close_FromOpen_TransitionsToClosed()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));

        task.Close();

        task.Status.Should().Be(TodoTaskStatus.Closed);
        task.ClosedAt.Should().NotBeNull();
        task.ClosedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Close_FromReopened_TransitionsToClosed()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();
        task.Reopen();

        task.Close();

        task.Status.Should().Be(TodoTaskStatus.Closed);
    }

    [Fact]
    public void Close_FromClosed_ThrowsInvalidTransition()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();

        var act = () => task.Close();

        act.Should().Throw<InvalidTransitionException>()
            .Where(e => e.From == TodoTaskStatus.Closed && e.To == TodoTaskStatus.Closed);
    }

    [Fact]
    public void Close_WithExplicitTimestamp_UsesProvidedValue()
    {
        var ts = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));

        task.Close(ts);

        task.ClosedAt.Should().Be(ts);
    }
}

public class TodoTaskReopenTests
{
    [Fact]
    public void Reopen_FromClosed_TransitionsToReopened()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();

        task.Reopen();

        task.Status.Should().Be(TodoTaskStatus.Reopened);
        task.ReopenedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reopen_FromOpen_ThrowsInvalidTransition()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));

        var act = () => task.Reopen();

        act.Should().Throw<InvalidTransitionException>()
            .Where(e => e.From == TodoTaskStatus.Open && e.To == TodoTaskStatus.Reopened);
    }

    [Fact]
    public void Reopen_FromReopened_ThrowsInvalidTransition()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();
        task.Reopen();

        var act = () => task.Reopen();

        act.Should().Throw<InvalidTransitionException>()
            .Where(e => e.From == TodoTaskStatus.Reopened && e.To == TodoTaskStatus.Reopened);
    }
}

public class TodoTaskUpdateTests
{
    [Fact]
    public void Update_ChangesNameDescriptionAndDate()
    {
        var task = TodoTask.Create("id1", "Old", "Old desc", new DateOnly(2026, 3, 1));

        task.Update("New", "New desc", new DateOnly(2026, 4, 1));

        task.Name.Should().Be("New");
        task.Description.Should().Be("New desc");
        task.CompletionDate.Should().Be(new DateOnly(2026, 4, 1));
    }

    [Fact]
    public void Update_WithNullDescription_ClearsDescription()
    {
        var task = TodoTask.Create("id1", "Task", "Some desc", new DateOnly(2026, 3, 1));

        task.Update("Task", null, new DateOnly(2026, 3, 1));

        task.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_Throws(string? name)
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        var act = () => task.Update(name!, null, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithNameOver200Chars_Throws()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        var act = () => task.Update(new string('x', 201), null, new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithDescriptionOver2000Chars_Throws()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        var act = () => task.Update("Task", new string('x', 2001), new DateOnly(2026, 3, 1));
        act.Should().Throw<ArgumentException>();
    }
}
