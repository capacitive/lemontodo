using System.Threading.Channels;
using LemonTodo.Domain.Events;
using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Infrastructure.Channels;

public class TaskEventChannel : ITaskEventChannel
{
    private readonly Channel<DomainEvent> _channel = Channel.CreateUnbounded<DomainEvent>(
        new UnboundedChannelOptions { SingleReader = true });

    public async ValueTask PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(domainEvent, ct);

    public IAsyncEnumerable<DomainEvent> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);
}
