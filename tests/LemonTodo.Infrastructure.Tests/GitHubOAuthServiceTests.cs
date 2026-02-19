using System.Net;
using System.Text.Json;
using FluentAssertions;
using LemonTodo.Infrastructure.Auth;

namespace LemonTodo.Infrastructure.Tests;

public class GitHubOAuthServiceTests
{
    private const string ClientId = "test-client-id";
    private const string ClientSecret = "test-client-secret";
    private const string RedirectUri = "http://localhost:5175/api/auth/github/callback";

    private static OAuthSettings CreateSettings() => new()
    {
        GitHub = new GitHubOAuthSettings
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
            TokenEndpoint = "https://github.com/login/oauth/access_token",
            UserInfoEndpoint = "https://api.github.com/user",
            UserEmailsEndpoint = "https://api.github.com/user/emails",
            Scopes = "read:user user:email"
        }
    };

    [Fact]
    public void GetAuthorizationUrl_BuildsCorrectUrl()
    {
        var settings = CreateSettings();
        var svc = new GitHubOAuthService(settings, new HttpClient(), RedirectUri);

        var url = svc.GetAuthorizationUrl("random-state-123");

        url.Should().StartWith("https://github.com/login/oauth/authorize?");
        url.Should().Contain($"client_id={ClientId}");
        url.Should().Contain("scope=read%3Auser%20user%3Aemail");
        url.Should().Contain("state=random-state-123");
        url.Should().Contain(Uri.EscapeDataString(RedirectUri));
    }

    [Fact]
    public async Task ExchangeCodeAsync_WithValidCode_ReturnsUserInfo()
    {
        var handler = new MockHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["https://github.com/login/oauth/access_token"] = JsonResponse(new
            {
                access_token = "gho_fake_token",
                token_type = "bearer",
                scope = "read:user,user:email"
            }),
            ["https://api.github.com/user"] = JsonResponse(new
            {
                id = 12345L,
                login = "testuser",
                name = "Test User",
                email = "test@example.com"
            })
        });

        var httpClient = new HttpClient(handler);
        var svc = new GitHubOAuthService(CreateSettings(), httpClient, RedirectUri);

        var result = await svc.ExchangeCodeAsync("valid-code");

        result.Id.Should().Be("12345");
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task ExchangeCodeAsync_FetchesEmailFromEmailsEndpoint_WhenNotPublic()
    {
        var handler = new MockHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["https://github.com/login/oauth/access_token"] = JsonResponse(new
            {
                access_token = "gho_fake_token",
                token_type = "bearer",
                scope = "read:user,user:email"
            }),
            ["https://api.github.com/user"] = JsonResponse(new
            {
                id = 99999L,
                login = "privateuser",
                name = (string?)null,
                email = (string?)null
            }),
            ["https://api.github.com/user/emails"] = JsonResponse(new[]
            {
                new { email = "secondary@example.com", primary = false, verified = true },
                new { email = "primary@example.com", primary = true, verified = true }
            })
        });

        var httpClient = new HttpClient(handler);
        var svc = new GitHubOAuthService(CreateSettings(), httpClient, RedirectUri);

        var result = await svc.ExchangeCodeAsync("valid-code");

        result.Id.Should().Be("99999");
        result.Email.Should().Be("primary@example.com");
        result.DisplayName.Should().Be("privateuser"); // Falls back to login
    }

    [Fact]
    public async Task ExchangeCodeAsync_WithErrorResponse_Throws()
    {
        var handler = new MockHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["https://github.com/login/oauth/access_token"] = JsonResponse(new
            {
                error = "bad_verification_code",
                error_description = "The code passed is incorrect or expired."
            })
        });

        var httpClient = new HttpClient(handler);
        var svc = new GitHubOAuthService(CreateSettings(), httpClient, RedirectUri);

        var act = () => svc.ExchangeCodeAsync("expired-code");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bad_verification_code*");
    }

    [Fact]
    public async Task ExchangeCodeAsync_NoVerifiedEmail_Throws()
    {
        var handler = new MockHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["https://github.com/login/oauth/access_token"] = JsonResponse(new
            {
                access_token = "gho_fake_token",
                token_type = "bearer"
            }),
            ["https://api.github.com/user"] = JsonResponse(new
            {
                id = 11111L,
                login = "noemailuser",
                name = "No Email",
                email = (string?)null
            }),
            ["https://api.github.com/user/emails"] = JsonResponse(new[]
            {
                new { email = "unverified@example.com", primary = true, verified = false }
            })
        });

        var httpClient = new HttpClient(handler);
        var svc = new GitHubOAuthService(CreateSettings(), httpClient, RedirectUri);

        var act = () => svc.ExchangeCodeAsync("valid-code");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No verified email*");
    }

    private static HttpResponseMessage JsonResponse(object body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json")
        };
}

/// <summary>
/// Simple HTTP message handler that returns canned responses based on request URL.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses;

    public MockHttpMessageHandler(Dictionary<string, HttpResponseMessage> responses)
    {
        _responses = responses;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.GetLeftPart(UriPartial.Path)
            ?? throw new InvalidOperationException("Request URI is null");

        if (_responses.TryGetValue(url, out var response))
            return Task.FromResult(response);

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
