namespace LemonTodo.Domain;

public class RefreshToken
{
    public string Id { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(string id, string tokenHash, string userId, DateTime expiresAt, DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration must be in the future.", nameof(expiresAt));

        return new RefreshToken
        {
            Id = id,
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            IsRevoked = false
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
