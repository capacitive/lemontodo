using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LemonTodo.Application.Interfaces;

namespace LemonTodo.Infrastructure.Auth;

public class GitHubOAuthService : IGitHubOAuthService
{
    private readonly OAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly string _redirectUri;

    public GitHubOAuthService(OAuthSettings settings, HttpClient httpClient, string redirectUri)
    {
        _settings = settings;
        _httpClient = httpClient;
        _redirectUri = redirectUri;
    }

    public string GetAuthorizationUrl(string state)
    {
        var gh = _settings.GitHub;
        return $"{gh.AuthorizationEndpoint}?client_id={Uri.EscapeDataString(gh.ClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
               $"&scope={Uri.EscapeDataString(gh.Scopes)}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<GitHubUserInfo> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var gh = _settings.GitHub;

        // Exchange authorization code for access token
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, gh.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = gh.ClientId,
                ["client_secret"] = gh.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = _redirectUri,
            })
        };
        tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var tokenResponse = await _httpClient.SendAsync(tokenRequest, ct);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Failed to parse GitHub token response.");

        if (!string.IsNullOrEmpty(tokenJson.Error))
            throw new InvalidOperationException($"GitHub OAuth error: {tokenJson.Error} - {tokenJson.ErrorDescription}");

        var accessToken = tokenJson.AccessToken
            ?? throw new InvalidOperationException("GitHub did not return an access token.");

        // Fetch user profile
        var userRequest = new HttpRequestMessage(HttpMethod.Get, gh.UserInfoEndpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        userRequest.Headers.UserAgent.Add(new ProductInfoHeaderValue("LemonTodo", "1.0"));

        var userResponse = await _httpClient.SendAsync(userRequest, ct);
        userResponse.EnsureSuccessStatusCode();

        var userInfo = await userResponse.Content.ReadFromJsonAsync<GitHubUserResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Failed to parse GitHub user response.");

        var email = userInfo.Email;

        // If email is not public, fetch from emails endpoint
        if (string.IsNullOrEmpty(email))
        {
            var emailRequest = new HttpRequestMessage(HttpMethod.Get, gh.UserEmailsEndpoint);
            emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            emailRequest.Headers.UserAgent.Add(new ProductInfoHeaderValue("LemonTodo", "1.0"));

            var emailResponse = await _httpClient.SendAsync(emailRequest, ct);
            emailResponse.EnsureSuccessStatusCode();

            var emails = await emailResponse.Content.ReadFromJsonAsync<List<GitHubEmailResponse>>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Failed to parse GitHub emails response.");

            email = emails.FirstOrDefault(e => e.Primary && e.Verified)?.Email
                ?? emails.FirstOrDefault(e => e.Verified)?.Email
                ?? throw new InvalidOperationException("No verified email found on GitHub account.");
        }

        var displayName = userInfo.Name ?? userInfo.Login ?? "GitHub User";
        var providerUserId = userInfo.Id.ToString();

        return new GitHubUserInfo(providerUserId, email, displayName);
    }

    private record GitHubTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("scope")] string? Scope,
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("error_description")] string? ErrorDescription);

    private record GitHubUserResponse(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("login")] string? Login,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email);

    private record GitHubEmailResponse(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("primary")] bool Primary,
        [property: JsonPropertyName("verified")] bool Verified);
}
