using LemonTodo.Domain.Exceptions;

namespace LemonTodo.Domain;

public class TodoTask
{
    public string Id { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly CompletionDate { get; private set; }
    public TodoTaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime? ReopenedAt { get; private set; }

    private TodoTask() { }

    public static TodoTask Create(string id, string name, string? description, DateOnly completionDate, DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters.", nameof(name));

        if (description is not null && description.Length > 2000)
            throw new ArgumentException("Description must not exceed 2000 characters.", nameof(description));

        return new TodoTask
        {
            Id = id,
            Name = name,
            Description = description,
            CompletionDate = completionDate,
            Status = TodoTaskStatus.Open,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    public void Close(DateTime? closedAt = null)
    {
        if (Status is not (TodoTaskStatus.Open or TodoTaskStatus.Reopened))
            throw new InvalidTransitionException(Status, TodoTaskStatus.Closed);

        Status = TodoTaskStatus.Closed;
        ClosedAt = closedAt ?? DateTime.UtcNow;
    }

    public void Reopen(DateTime? reopenedAt = null)
    {
        if (Status is not TodoTaskStatus.Closed)
            throw new InvalidTransitionException(Status, TodoTaskStatus.Reopened);

        Status = TodoTaskStatus.Reopened;
        ReopenedAt = reopenedAt ?? DateTime.UtcNow;
    }

    public void Update(string name, string? description, DateOnly completionDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters.", nameof(name));

        if (description is not null && description.Length > 2000)
            throw new ArgumentException("Description must not exceed 2000 characters.", nameof(description));

        Name = name;
        Description = description;
        CompletionDate = completionDate;
    }
}
