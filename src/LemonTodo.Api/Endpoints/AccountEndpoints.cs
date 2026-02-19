using System.Security.Claims;
using FluentValidation;
using LemonTodo.Api.Auth;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;

namespace LemonTodo.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/account").WithTags("Account").RequireAuthorization();

        group.MapGet("/profile", async (ClaimsPrincipal user, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var profile = await svc.GetProfileAsync(user.GetUserId(), ct);
                return Results.Ok(profile);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapPut("/profile", async (UpdateProfileRequest request, ClaimsPrincipal user,
            IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var profile = await svc.UpdateProfileAsync(user.GetUserId(), request, ct);
                return Results.Ok(profile);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/change-password", async (ChangePasswordRequest request,
            IValidator<ChangePasswordRequest> validator, ClaimsPrincipal user,
            IAccountService svc, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                await svc.ChangePasswordAsync(user.GetUserId(), request, ct);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 401);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapPost("/2fa/setup", async (ClaimsPrincipal user, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var setup = await svc.SetupTwoFactorAsync(user.GetUserId(), ct);
                return Results.Ok(setup);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/2fa/enable", async (TwoFactorVerifyRequest request, ClaimsPrincipal user,
            IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                await svc.EnableTwoFactorAsync(user.GetUserId(), request.Code, ct);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 401);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/2fa/disable", async (TwoFactorVerifyRequest request, ClaimsPrincipal user,
            IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                await svc.DisableTwoFactorAsync(user.GetUserId(), request.Code, ct);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 401);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/api-key", async (ClaimsPrincipal user, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var key = await svc.GenerateApiKeyAsync(user.GetUserId(), ct);
                return Results.Created("/api/account/api-key", key);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/api-key", async (ClaimsPrincipal user, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                await svc.RevokeApiKeyAsync(user.GetUserId(), ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/preferences", async (ClaimsPrincipal user, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var prefs = await svc.GetBoardPreferencesAsync(user.GetUserId(), ct);
                return Results.Ok(new { preferences = prefs });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapPut("/preferences", async (PreferencesRequest request, ClaimsPrincipal user,
            IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var prefs = await svc.UpdateBoardPreferencesAsync(user.GetUserId(), request.Preferences, ct);
                return Results.Ok(new { preferences = prefs });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });
    }
}

public record PreferencesRequest(string? Preferences);
