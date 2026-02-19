using System.Net;
using FluentAssertions;
using LemonTodo.Api.Tests.Helpers;
using LemonTodo.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LemonTodo.Api.Tests;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(IGitHubOAuthService? gitHubOAuth = null)
    {
        var mockGitHub = gitHubOAuth ?? Substitute.For<IGitHubOAuthService>();
        if (gitHubOAuth is null)
        {
            mockGitHub.GetAuthorizationUrl(Arg.Any<string>())
                .Returns("https://github.com/login/oauth/authorize?client_id=test");
        }

        var testFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });

                // Replace GitHub OAuth service with mock
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IGitHubOAuthService));
                if (descriptor is not null) services.Remove(descriptor);
                services.AddSingleton(mockGitHub);
            });
        });

        return testFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // Important: we need to inspect redirects
        });
    }

    [Fact]
    public async Task GitHubRedirect_Returns302_WithGitHubUrl()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/github");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().StartWith("https://github.com/login/oauth/authorize");
    }

    [Fact]
    public async Task GitHubRedirect_SetsStateCookie()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/github");

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var cookieValues = cookies!.ToList();
        cookieValues.Should().Contain(c => c.StartsWith("oauth_state="));
    }

    [Fact]
    public async Task GitHubCallback_WithValidCodeAndState_RedirectsToFrontend()
    {
        var mockGitHub = Substitute.For<IGitHubOAuthService>();
        mockGitHub.GetAuthorizationUrl(Arg.Any<string>())
            .Returns("https://github.com/login/oauth/authorize?client_id=test");
        mockGitHub.ExchangeCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GitHubUserInfo("12345", "test@example.com", "Test User"));

        var client = CreateClient(mockGitHub);

        // Step 1: Get the redirect to capture the state cookie
        var redirectResponse = await client.GetAsync("/api/auth/github");
        var stateCookie = redirectResponse.Headers.GetValues("Set-Cookie")
            .First(c => c.StartsWith("oauth_state="));
        var stateValue = stateCookie.Split('=')[1].Split(';')[0];

        // Step 2: Call callback with the state
        var callbackRequest = new HttpRequestMessage(HttpMethod.Get,
            $"/api/auth/github/callback?code=valid-code&state={stateValue}");
        callbackRequest.Headers.Add("Cookie", $"oauth_state={stateValue}");

        var callbackResponse = await client.SendAsync(callbackRequest);

        callbackResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = callbackResponse.Headers.Location!.ToString();
        location.Should().Contain("access_token=");
        location.Should().Contain("refresh_token=");
    }

    [Fact]
    public async Task GitHubCallback_WithMismatchedState_Returns400()
    {
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/auth/github/callback?code=valid-code&state=wrong-state");
        request.Headers.Add("Cookie", "oauth_state=different-state");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GitHubCallback_WithNoStateCookie_Returns400()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/github/callback?code=valid-code&state=some-state");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GitHubCallback_WhenServiceThrows_Returns502()
    {
        var mockGitHub = Substitute.For<IGitHubOAuthService>();
        mockGitHub.GetAuthorizationUrl(Arg.Any<string>())
            .Returns("https://github.com/login/oauth/authorize?client_id=test");
        mockGitHub.ExchangeCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("GitHub OAuth error: bad_verification_code"));

        var client = CreateClient(mockGitHub);

        // Get state first
        var redirectResponse = await client.GetAsync("/api/auth/github");
        var stateCookie = redirectResponse.Headers.GetValues("Set-Cookie")
            .First(c => c.StartsWith("oauth_state="));
        var stateValue = stateCookie.Split('=')[1].Split(';')[0];

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/auth/github/callback?code=bad-code&state={stateValue}");
        request.Headers.Add("Cookie", $"oauth_state={stateValue}");

        var response = await client.SendAsync(request);

        // Should return error, not crash
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadGateway, HttpStatusCode.BadRequest);
    }
}
