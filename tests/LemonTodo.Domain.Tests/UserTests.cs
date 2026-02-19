using FluentAssertions;
using LemonTodo.Domain;

namespace LemonTodo.Domain.Tests;

public class UserCreationTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsUser()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.Id.Should().Be("usr-123");
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("Test User");
        user.PasswordHash.Should().BeNull();
        user.TwoFactorEnabled.Should().BeFalse();
        user.TotpSecret.Should().BeNull();
        user.ApiKeyHash.Should().BeNull();
        user.BoardPreferencesJson.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.LastLoginAt.Should().BeNull();
        user.ExternalLogins.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithPassword_SetsPasswordHash()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User", "hashed-pw");
        user.PasswordHash.Should().Be("hashed-pw");
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCase()
    {
        var user = User.Create("usr-123", "Test@EXAMPLE.com", "Test User");
        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidId_Throws(string? id)
    {
        var act = () => User.Create(id!, "test@example.com", "Test User");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_Throws(string? email)
    {
        var act = () => User.Create("usr-123", email!, "Test User");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDisplayName_Throws(string? name)
    {
        var act = () => User.Create("usr-123", "test@example.com", name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDisplayNameOver100Chars_Throws()
    {
        var longName = new string('x', 101);
        var act = () => User.Create("usr-123", "test@example.com", longName);
        act.Should().Throw<ArgumentException>().WithMessage("*100*");
    }

    [Fact]
    public void Create_WithEmailOver254Chars_Throws()
    {
        var longEmail = new string('x', 251) + "@a.b";
        var act = () => User.Create("usr-123", longEmail, "Test User");
        act.Should().Throw<ArgumentException>().WithMessage("*254*");
    }
}

public class UserProfileTests
{
    [Fact]
    public void UpdateProfile_ChangesDisplayName()
    {
        var user = User.Create("usr-123", "test@example.com", "Old Name");

        user.UpdateProfile("New Name");

        user.DisplayName.Should().Be("New Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithInvalidName_Throws(string? name)
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");
        var act = () => user.UpdateProfile(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetPasswordHash_SetsHash()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.SetPasswordHash("new-hash");

        user.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public void SetBoardPreferences_SetsJson()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.SetBoardPreferences("{\"theme\":\"dark\"}");

        user.BoardPreferencesJson.Should().Be("{\"theme\":\"dark\"}");
    }
}

public class UserTwoFactorTests
{
    [Fact]
    public void EnableTwoFactor_SetsSecretAndFlag()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.EnableTwoFactor("JBSWY3DPEHPK3PXP");

        user.TwoFactorEnabled.Should().BeTrue();
        user.TotpSecret.Should().Be("JBSWY3DPEHPK3PXP");
    }

    [Fact]
    public void EnableTwoFactor_WhenAlreadyEnabled_Throws()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");
        user.EnableTwoFactor("JBSWY3DPEHPK3PXP");

        var act = () => user.EnableTwoFactor("ANOTHER_SECRET");

        act.Should().Throw<InvalidOperationException>().WithMessage("*already enabled*");
    }

    [Fact]
    public void DisableTwoFactor_ClearsSecretAndFlag()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");
        user.EnableTwoFactor("JBSWY3DPEHPK3PXP");

        user.DisableTwoFactor();

        user.TwoFactorEnabled.Should().BeFalse();
        user.TotpSecret.Should().BeNull();
    }

    [Fact]
    public void DisableTwoFactor_WhenNotEnabled_Throws()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        var act = () => user.DisableTwoFactor();

        act.Should().Throw<InvalidOperationException>().WithMessage("*not enabled*");
    }
}

public class UserExternalLoginTests
{
    [Fact]
    public void AddExternalLogin_AddsToCollection()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User", "hash");

        user.AddExternalLogin("Google", "google-user-id-123");

        user.ExternalLogins.Should().HaveCount(1);
        user.ExternalLogins[0].Provider.Should().Be("Google");
        user.ExternalLogins[0].ProviderUserId.Should().Be("google-user-id-123");
        user.ExternalLogins[0].UserId.Should().Be("usr-123");
    }

    [Fact]
    public void AddExternalLogin_DuplicateProvider_Throws()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User", "hash");
        user.AddExternalLogin("Google", "google-user-id-123");

        var act = () => user.AddExternalLogin("Google", "different-id");

        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void RemoveExternalLogin_RemovesFromCollection()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User", "hash");
        user.AddExternalLogin("Google", "google-user-id-123");

        user.RemoveExternalLogin("Google");

        user.ExternalLogins.Should().BeEmpty();
    }

    [Fact]
    public void RemoveExternalLogin_WhenOnlyLoginMethod_Throws()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");
        user.AddExternalLogin("Google", "google-user-id-123");

        var act = () => user.RemoveExternalLogin("Google");

        act.Should().Throw<InvalidOperationException>().WithMessage("*only login method*");
    }

    [Fact]
    public void RemoveExternalLogin_NonExistentProvider_Throws()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User", "hash");

        var act = () => user.RemoveExternalLogin("GitHub");

        act.Should().Throw<InvalidOperationException>().WithMessage("*No external login*");
    }
}

public class UserApiKeyTests
{
    [Fact]
    public void SetApiKeyHash_SetsHash()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");

        user.SetApiKeyHash("sha256-hash");

        user.ApiKeyHash.Should().Be("sha256-hash");
    }

    [Fact]
    public void RevokeApiKey_ClearsHash()
    {
        var user = User.Create("usr-123", "test@example.com", "Test User");
        user.SetApiKeyHash("sha256-hash");

        user.RevokeApiKey();

        user.ApiKeyHash.Should().BeNull();
    }
}
