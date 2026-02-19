namespace LemonTodo.Domain.Events;

public abstract record DomainEvent(string TaskId, DateTime OccurredAt);

public record TaskStartedEvent(string TaskId, DateTime OccurredAt) : DomainEvent(TaskId, OccurredAt);

public record TaskClosedEvent(string TaskId, DateTime OccurredAt) : DomainEvent(TaskId, OccurredAt);

public record TaskReopenedEvent(string TaskId, DateTime OccurredAt) : DomainEvent(TaskId, OccurredAt);
