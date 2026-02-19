using FluentAssertions;
using LemonTodo.Domain;
using LemonTodo.Infrastructure.Data;
using LemonTodo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly UserDbContext _db;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new UserDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _repo = new UserRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAndGetById_RoundTrip()
    {
        var user = User.Create("usr-1", "test@example.com", "Test User", "hash");
        await _repo.AddAsync(user);

        var found = await _repo.GetByIdAsync("usr-1");

        found.Should().NotBeNull();
        found!.Email.Should().Be("test@example.com");
        found.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByEmail_FindsUser()
    {
        var user = User.Create("usr-1", "test@example.com", "Test User");
        await _repo.AddAsync(user);

        var found = await _repo.GetByEmailAsync("TEST@EXAMPLE.COM");

        found.Should().NotBeNull();
        found!.Id.Should().Be("usr-1");
    }

    [Fact]
    public async Task GetByEmail_NotFound_ReturnsNull()
    {
        var found = await _repo.GetByEmailAsync("nobody@example.com");
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalLogin_FindsUser()
    {
        var user = User.Create("usr-1", "test@example.com", "Test User", "hash");
        user.AddExternalLogin("Google", "google-123");
        await _repo.AddAsync(user);

        var found = await _repo.GetByExternalLoginAsync("Google", "google-123");

        found.Should().NotBeNull();
        found!.Id.Should().Be("usr-1");
        found.ExternalLogins.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByExternalLogin_NotFound_ReturnsNull()
    {
        var found = await _repo.GetByExternalLoginAsync("GitHub", "nonexistent");
        found.Should().BeNull();
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var user = User.Create("usr-1", "test@example.com", "Old Name");
        await _repo.AddAsync(user);

        user.UpdateProfile("New Name");
        await _repo.UpdateAsync(user);

        var found = await _repo.GetByIdAsync("usr-1");
        found!.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public async Task AddDuplicateEmail_Throws()
    {
        var user1 = User.Create("usr-1", "test@example.com", "User 1");
        var user2 = User.Create("usr-2", "test@example.com", "User 2");
        await _repo.AddAsync(user1);

        var act = async () => await _repo.AddAsync(user2);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExternalLogins_PersistedWithUser()
    {
        var user = User.Create("usr-1", "test@example.com", "Test User", "hash");
        user.AddExternalLogin("Google", "g-123");
        user.AddExternalLogin("GitHub", "gh-456");
        await _repo.AddAsync(user);

        var found = await _repo.GetByIdAsync("usr-1");

        found!.ExternalLogins.Should().HaveCount(2);
        found.ExternalLogins.Should().Contain(e => e.Provider == "Google");
        found.ExternalLogins.Should().Contain(e => e.Provider == "GitHub");
    }
}

public class RefreshTokenRepositoryTests : IDisposable
{
    private readonly UserDbContext _db;
    private readonly RefreshTokenRepository _repo;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new UserDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _repo = new RefreshTokenRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAndGetByTokenHash_RoundTrip()
    {
        var token = RefreshToken.Create("rt-1", "hash-abc", "usr-1", DateTime.UtcNow.AddDays(30));
        await _repo.AddAsync(token);

        var found = await _repo.GetByTokenHashAsync("hash-abc");

        found.Should().NotBeNull();
        found!.UserId.Should().Be("usr-1");
        found.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAllForUser_RevokesAllTokens()
    {
        var t1 = RefreshToken.Create("rt-1", "hash-1", "usr-1", DateTime.UtcNow.AddDays(30));
        var t2 = RefreshToken.Create("rt-2", "hash-2", "usr-1", DateTime.UtcNow.AddDays(30));
        var t3 = RefreshToken.Create("rt-3", "hash-3", "usr-2", DateTime.UtcNow.AddDays(30));
        await _repo.AddAsync(t1);
        await _repo.AddAsync(t2);
        await _repo.AddAsync(t3);

        await _repo.RevokeAllForUserAsync("usr-1");

        var found1 = await _repo.GetByTokenHashAsync("hash-1");
        var found2 = await _repo.GetByTokenHashAsync("hash-2");
        var found3 = await _repo.GetByTokenHashAsync("hash-3");

        found1!.IsRevoked.Should().BeTrue();
        found2!.IsRevoked.Should().BeTrue();
        found3!.IsRevoked.Should().BeFalse();
    }
}
