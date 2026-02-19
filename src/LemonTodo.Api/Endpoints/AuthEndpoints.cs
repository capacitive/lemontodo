using FluentValidation;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;

namespace LemonTodo.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequest request, IValidator<RegisterRequest> validator,
            IAuthService svc, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var response = await svc.RegisterAsync(request, ct);
                return Results.Created("/api/account/profile", response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService svc, CancellationToken ct) =>
        {
            try
            {
                var response = await svc.LoginAsync(request, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 401);
            }
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService svc, CancellationToken ct) =>
        {
            try
            {
                var response = await svc.RefreshAsync(request.RefreshToken, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Json(new { error = "Invalid or expired refresh token." }, statusCode: 401);
            }
        });

        group.MapPost("/logout", async (RefreshTokenRequest request, IAuthService svc, CancellationToken ct) =>
        {
            await svc.RevokeRefreshTokenAsync(request.RefreshToken, ct);
            return Results.NoContent();
        });

        // OAuth endpoints - redirect to provider
        group.MapGet("/google", (HttpContext ctx) =>
        {
            // TODO: Implement Google OAuth redirect
            return Results.StatusCode(501);
        });

        group.MapGet("/google/callback", async (string code, string? state,
            IAuthService svc, CancellationToken ct) =>
        {
            // TODO: Exchange code for token, extract user info, call OAuthLoginAsync
            return await Task.FromResult(Results.StatusCode(501));
        });

        group.MapGet("/github", (HttpContext ctx) =>
        {
            // TODO: Implement GitHub OAuth redirect
            return Results.StatusCode(501);
        });

        group.MapGet("/github/callback", async (string code, string? state,
            IAuthService svc, CancellationToken ct) =>
        {
            // TODO: Exchange code for token, extract user info, call OAuthLoginAsync
            return await Task.FromResult(Results.StatusCode(501));
        });
    }
}
