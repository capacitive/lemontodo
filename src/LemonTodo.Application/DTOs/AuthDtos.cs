namespace LemonTodo.Application.DTOs;

public record RegisterRequest(string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password, string? TotpCode = null);

public record AuthResponse(string AccessToken, string RefreshToken, UserProfile User, bool RequiresTwoFactor = false);

public record UserProfile(
    string Id,
    string Email,
    string DisplayName,
    bool TwoFactorEnabled,
    IReadOnlyList<string> LinkedProviders,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record TwoFactorSetupResponse(string SharedKey, string QrCodeUri);

public record TwoFactorVerifyRequest(string Code);

public record RefreshTokenRequest(string RefreshToken);

public record UpdateProfileRequest(string DisplayName);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ApiKeyResponse(string ApiKey);

public record OAuthCallbackRequest(string Provider, string Code, string RedirectUri);
