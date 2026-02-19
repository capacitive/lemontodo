using LemonTodo.Domain;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
