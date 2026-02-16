using FluentAssertions;
using LemonTodo.Domain.Events;
using LemonTodo.Infrastructure.Channels;

namespace LemonTodo.Infrastructure.Tests;

public class TaskEventChannelTests
{
    [Fact]
    public async Task PublishAndRead_DeliversEvent()
    {
        var channel = new TaskEventChannel();
        var evt = new TaskClosedEvent("id1", DateTime.UtcNow);

        await channel.PublishAsync(evt);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        DomainEvent? received = null;
        await foreach (var e in channel.ReadAllAsync(cts.Token))
        {
            received = e;
            break;
        }

        received.Should().NotBeNull();
        received.Should().BeOfType<TaskClosedEvent>();
        ((TaskClosedEvent)received!).TaskId.Should().Be("id1");
    }

    [Fact]
    public async Task PublishMultiple_ReadsInOrder()
    {
        var channel = new TaskEventChannel();
        var evt1 = new TaskClosedEvent("id1", DateTime.UtcNow);
        var evt2 = new TaskReopenedEvent("id2", DateTime.UtcNow);

        await channel.PublishAsync(evt1);
        await channel.PublishAsync(evt2);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var events = new List<DomainEvent>();
        await foreach (var e in channel.ReadAllAsync(cts.Token))
        {
            events.Add(e);
            if (events.Count == 2) break;
        }

        events[0].Should().BeOfType<TaskClosedEvent>();
        events[1].Should().BeOfType<TaskReopenedEvent>();
    }
}
