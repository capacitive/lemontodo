using System.Security.Cryptography;
using System.Text;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Mapping;
using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITotpService _totpService;
    private readonly ITokenGenerator _tokenGenerator;

    public AccountService(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        ITotpService totpService,
        ITokenGenerator tokenGenerator)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _totpService = totpService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<UserProfile> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");
        return user.ToProfile();
    }

    public async Task<UserProfile> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.UpdateProfile(request.DisplayName);
        await _userRepo.UpdateAsync(user, ct);
        return user.ToProfile();
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.PasswordHash is not null && !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _userRepo.UpdateAsync(user, ct);
    }

    public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.TwoFactorEnabled)
            throw new InvalidOperationException("Two-factor authentication is already enabled.");

        var secret = _totpService.GenerateSecret();
        var qrUri = _totpService.GetQrCodeUri(secret, user.Email);

        return new TwoFactorSetupResponse(secret, qrUri);
    }

    public async Task EnableTwoFactorAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var setup = await SetupTwoFactorAsync(userId, ct);

        if (!_totpService.ValidateCode(setup.SharedKey, code))
            throw new UnauthorizedAccessException("Invalid verification code.");

        user.EnableTwoFactor(setup.SharedKey);
        await _userRepo.UpdateAsync(user, ct);
    }

    public async Task DisableTwoFactorAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.TwoFactorEnabled || user.TotpSecret is null)
            throw new InvalidOperationException("Two-factor authentication is not enabled.");

        if (!_totpService.ValidateCode(user.TotpSecret, code))
            throw new UnauthorizedAccessException("Invalid verification code.");

        user.DisableTwoFactor();
        await _userRepo.UpdateAsync(user, ct);
    }

    public async Task<ApiKeyResponse> GenerateApiKeyAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var apiKey = GenerateRandomApiKey();
        var hash = _tokenGenerator.HashToken(apiKey);
        user.SetApiKeyHash(hash);
        await _userRepo.UpdateAsync(user, ct);

        return new ApiKeyResponse(apiKey);
    }

    public async Task RevokeApiKeyAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.RevokeApiKey();
        await _userRepo.UpdateAsync(user, ct);
    }

    public async Task<string?> GetBoardPreferencesAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");
        return user.BoardPreferencesJson;
    }

    public async Task<string?> UpdateBoardPreferencesAsync(string userId, string? json, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.SetBoardPreferences(json);
        await _userRepo.UpdateAsync(user, ct);
        return user.BoardPreferencesJson;
    }

    private static string GenerateRandomApiKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return $"lt_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }
}
