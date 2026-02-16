using LemonTodo.Application.Interfaces;
using LemonTodo.Domain.Exceptions;

namespace LemonTodo.Api.Endpoints;

public static class ArchiveEndpoints
{
    public static void MapArchiveEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/archive").WithTags("Archive");

        group.MapGet("/", async (string? q, int? page, int? pageSize,
            IArchiveService svc, CancellationToken ct) =>
        {
            var result = await svc.SearchAsync(q, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id}", async (string id, IArchiveService svc, CancellationToken ct) =>
        {
            var task = await svc.GetByIdAsync(id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPatch("/{id}/restore", async (string id, IArchiveService svc, CancellationToken ct) =>
        {
            try
            {
                var task = await svc.RestoreAsync(id, ct);
                return Results.Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidTransitionException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });
    }
}
