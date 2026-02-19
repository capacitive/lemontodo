using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _db;

    public UserRepository(UserDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Users.Include(u => u.ExternalLogins).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken ct = default)
        => await _db.Users.Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.ExternalLogins.Any(
                e => e.Provider == provider && e.ProviderUserId == providerUserId), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
