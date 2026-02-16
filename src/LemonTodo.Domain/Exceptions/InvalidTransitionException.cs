namespace LemonTodo.Domain.Exceptions;

public class InvalidTransitionException : InvalidOperationException
{
    public TodoTaskStatus From { get; }
    public TodoTaskStatus To { get; }

    public InvalidTransitionException(TodoTaskStatus from, TodoTaskStatus to)
        : base($"Invalid status transition from {from} to {to}.")
    {
        From = from;
        To = to;
    }
}
