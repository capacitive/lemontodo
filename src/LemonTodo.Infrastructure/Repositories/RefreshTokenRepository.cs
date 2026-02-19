using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UserDbContext _db;

    public RefreshTokenRepository(UserDbContext db) => _db = db;

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Revoke();

        await _db.SaveChangesAsync(ct);
    }
}
