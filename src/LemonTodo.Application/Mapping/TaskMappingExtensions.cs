using LemonTodo.Application.DTOs;
using LemonTodo.Domain;

namespace LemonTodo.Application.Mapping;

public static class TaskMappingExtensions
{
    public static TaskResponse ToResponse(this TodoTask task)
        => new(
            task.Id,
            task.Name,
            task.Description,
            task.CompletionDate,
            task.Status.ToString(),
            task.CreatedAt,
            task.StartedAt,
            task.ClosedAt,
            task.ReopenedAt);
}
