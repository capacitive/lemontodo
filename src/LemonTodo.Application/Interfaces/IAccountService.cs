using LemonTodo.Application.DTOs;

namespace LemonTodo.Application.Interfaces;

public interface IAccountService
{
    Task<UserProfile> GetProfileAsync(string userId, CancellationToken ct = default);
    Task<UserProfile> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<TwoFactorSetupResponse> SetupTwoFactorAsync(string userId, CancellationToken ct = default);
    Task EnableTwoFactorAsync(string userId, string code, CancellationToken ct = default);
    Task DisableTwoFactorAsync(string userId, string code, CancellationToken ct = default);
    Task<ApiKeyResponse> GenerateApiKeyAsync(string userId, CancellationToken ct = default);
    Task RevokeApiKeyAsync(string userId, CancellationToken ct = default);
    Task<string?> GetBoardPreferencesAsync(string userId, CancellationToken ct = default);
    Task<string?> UpdateBoardPreferencesAsync(string userId, string? json, CancellationToken ct = default);
}
