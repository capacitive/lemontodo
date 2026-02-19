using System.Security.Claims;
using FluentValidation;
using LemonTodo.Api.Auth;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;
using LemonTodo.Domain.Exceptions;

namespace LemonTodo.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            var tasks = await svc.GetAllAsync(user.GetUserId(), ct);
            return Results.Ok(tasks);
        });

        group.MapGet("/{id}", async (string id, ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            var task = await svc.GetByIdAsync(user.GetUserId(), id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPost("/", async (CreateTaskRequest request, IValidator<CreateTaskRequest> validator,
            ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var task = await svc.CreateAsync(user.GetUserId(), request, ct);
            return Results.Created($"/api/tasks/{task.Id}", task);
        });

        group.MapPut("/{id}", async (string id, UpdateTaskRequest request,
            IValidator<UpdateTaskRequest> validator, ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var task = await svc.UpdateAsync(user.GetUserId(), id, request, ct);
                return Results.Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapPatch("/{id}/close", async (string id, ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            try
            {
                var task = await svc.CloseAsync(user.GetUserId(), id, ct);
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

        group.MapPatch("/{id}/reopen", async (string id, ClaimsPrincipal user, ITaskService svc, CancellationToken ct) =>
        {
            try
            {
                var task = await svc.ReopenAsync(user.GetUserId(), id, ct);
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
