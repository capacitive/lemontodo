namespace LemonTodo.Infrastructure.Auth;

public class OAuthSettings
{
    public GitHubOAuthSettings GitHub { get; set; } = new();
}

public class GitHubOAuthSettings
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string AuthorizationEndpoint { get; set; } = "https://github.com/login/oauth/authorize";
    public string TokenEndpoint { get; set; } = "https://github.com/login/oauth/access_token";
    public string UserInfoEndpoint { get; set; } = "https://api.github.com/user";
    public string UserEmailsEndpoint { get; set; } = "https://api.github.com/user/emails";
    public string Scopes { get; set; } = "read:user user:email";
}
