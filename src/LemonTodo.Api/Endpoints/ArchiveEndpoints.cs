using System.Security.Claims;
using LemonTodo.Api.Auth;
using LemonTodo.Application.Interfaces;
using LemonTodo.Domain.Exceptions;

namespace LemonTodo.Api.Endpoints;

public static class ArchiveEndpoints
{
    public static void MapArchiveEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/archive").WithTags("Archive").RequireAuthorization();

        group.MapGet("/", async (string? q, int? page, int? pageSize,
            ClaimsPrincipal user, IArchiveService svc, CancellationToken ct) =>
        {
            var result = await svc.SearchAsync(user.GetUserId(), q, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id}", async (string id, ClaimsPrincipal user, IArchiveService svc, CancellationToken ct) =>
        {
            var task = await svc.GetByIdAsync(user.GetUserId(), id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPatch("/{id}/restore", async (string id, ClaimsPrincipal user, IArchiveService svc, CancellationToken ct) =>
        {
            try
            {
                var task = await svc.RestoreAsync(user.GetUserId(), id, ct);
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

        group.MapDelete("/{id}", async (string id, ClaimsPrincipal user, IArchiveService svc, CancellationToken ct) =>
        {
            try
            {
                await svc.DeleteAsync(user.GetUserId(), id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/purge", async (ClaimsPrincipal user, IArchiveService svc, CancellationToken ct) =>
        {
            await svc.PurgeAllAsync(user.GetUserId(), ct);
            return Results.NoContent();
        });
    }
}
