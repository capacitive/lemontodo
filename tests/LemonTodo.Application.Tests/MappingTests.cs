using FluentAssertions;
using LemonTodo.Application.Mapping;
using LemonTodo.Domain;

namespace LemonTodo.Application.Tests;

public class MappingTests
{
    [Fact]
    public void ToResponse_MapsAllFields()
    {
        var ts = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var task = TodoTask.Create("id1", "Task", "Desc", new DateOnly(2026, 3, 1), ts);

        var response = task.ToResponse();

        response.Id.Should().Be("id1");
        response.Name.Should().Be("Task");
        response.Description.Should().Be("Desc");
        response.CompletionDate.Should().Be(new DateOnly(2026, 3, 1));
        response.Status.Should().Be("Open");
        response.CreatedAt.Should().Be(ts);
        response.ClosedAt.Should().BeNull();
        response.ReopenedAt.Should().BeNull();
    }

    [Fact]
    public void ToResponse_AfterClose_ReflectsClosedStatus()
    {
        var task = TodoTask.Create("id1", "Task", null, new DateOnly(2026, 3, 1));
        task.Close();

        var response = task.ToResponse();

        response.Status.Should().Be("Closed");
        response.ClosedAt.Should().NotBeNull();
    }
}
