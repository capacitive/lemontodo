namespace LemonTodo.Application.DTOs;

public record CreateTaskRequest(string Name, string? Description, DateOnly CompletionDate);

public record UpdateTaskRequest(string Name, string? Description, DateOnly CompletionDate);

public record TaskResponse(
    string Id,
    string Name,
    string? Description,
    DateOnly CompletionDate,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? ClosedAt,
    DateTime? ReopenedAt);

public record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
