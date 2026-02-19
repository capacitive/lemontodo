namespace LemonTodo.Domain;

public class User
{
    public string Id { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public string? TotpSecret { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? ApiKeyHash { get; private set; }
    public string? BoardPreferencesJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<ExternalLogin> _externalLogins = new();
    public IReadOnlyList<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();

    private User() { }

    public static User Create(string id, string email, string displayName, string? passwordHash = null, DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        if (displayName.Length > 100)
            throw new ArgumentException("Display name must not exceed 100 characters.", nameof(displayName));

        if (email.Length > 254)
            throw new ArgumentException("Email must not exceed 254 characters.", nameof(email));

        return new User
        {
            Id = id,
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            PasswordHash = passwordHash,
            TwoFactorEnabled = false,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    public void UpdateProfile(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        if (displayName.Length > 100)
            throw new ArgumentException("Display name must not exceed 100 characters.", nameof(displayName));

        DisplayName = displayName;
    }

    public void SetPasswordHash(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        PasswordHash = hash;
    }

    public void EnableTwoFactor(string totpSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(totpSecret);

        if (TwoFactorEnabled)
            throw new InvalidOperationException("Two-factor authentication is already enabled.");

        TotpSecret = totpSecret;
        TwoFactorEnabled = true;
    }

    public void DisableTwoFactor()
    {
        if (!TwoFactorEnabled)
            throw new InvalidOperationException("Two-factor authentication is not enabled.");

        TotpSecret = null;
        TwoFactorEnabled = false;
    }

    public void SetApiKeyHash(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ApiKeyHash = hash;
    }

    public void RevokeApiKey()
    {
        ApiKeyHash = null;
    }

    public void SetBoardPreferences(string? json)
    {
        BoardPreferencesJson = json;
    }

    public void AddExternalLogin(string provider, string providerUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerUserId);

        if (_externalLogins.Any(e => e.Provider == provider))
            throw new InvalidOperationException($"External login for provider '{provider}' already exists.");

        _externalLogins.Add(ExternalLogin.Create(provider, providerUserId, Id));
    }

    public void RemoveExternalLogin(string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        var login = _externalLogins.FirstOrDefault(e => e.Provider == provider)
            ?? throw new InvalidOperationException($"No external login found for provider '{provider}'.");

        if (PasswordHash is null && _externalLogins.Count == 1)
            throw new InvalidOperationException("Cannot remove the only login method. Set a password first.");

        _externalLogins.Remove(login);
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
