namespace LemonTodo.Domain;

public class ExternalLogin
{
    public string Provider { get; private set; } = default!;
    public string ProviderUserId { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public DateTime LinkedAt { get; private set; }

    private ExternalLogin() { }

    internal static ExternalLogin Create(string provider, string providerUserId, string userId)
    {
        return new ExternalLogin
        {
            Provider = provider,
            ProviderUserId = providerUserId,
            UserId = userId,
            LinkedAt = DateTime.UtcNow
        };
    }
}
