namespace LemonTodo.Application.Interfaces;

public record GitHubUserInfo(string Id, string Email, string DisplayName);

public interface IGitHubOAuthService
{
    string GetAuthorizationUrl(string state);
    Task<GitHubUserInfo> ExchangeCodeAsync(string code, CancellationToken ct = default);
}
