using System.Security.Claims;
using System.Text.Encodings.Web;
using LemonTodo.Application.Interfaces;
using LemonTodo.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LemonTodo.Api.Auth;

public class ApiKeyAuthOptions : AuthenticationSchemeOptions { }

public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly IServiceScopeFactory _scopeFactory;

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IServiceScopeFactory scopeFactory)
        : base(options, logger, encoder)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyHeader))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyHeader.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("API key is empty.");

        using var scope = _scopeFactory.CreateScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<ITokenGenerator>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var hash = tokenGenerator.HashToken(apiKey);

        // Search for user with matching API key hash
        // This is a simple approach — for production, you'd want an indexed lookup
        // For now, we'll check via the repository
        // TODO: Add GetByApiKeyHashAsync to IUserRepository for efficiency
        await Task.CompletedTask; // placeholder for async

        return AuthenticateResult.NoResult();
    }
}
