using LemonTodo.Domain.Events;

namespace LemonTodo.Domain.Interfaces;

public interface ITaskEventChannel
{
    ValueTask PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
    IAsyncEnumerable<DomainEvent> ReadAllAsync(CancellationToken ct = default);
}
