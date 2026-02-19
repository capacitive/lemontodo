using LemonTodo.Application.DTOs;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Mapping;
using LemonTodo.Domain;
using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IIdGenerator _idGen;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITotpService _totpService;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IIdGenerator idGen,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        ITotpService totpService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _idGen = idGen;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _totpService = totpService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(_idGen.NewId(), request.Email, request.DisplayName, hash);
        await _userRepo.AddAsync(user, ct);

        user.RecordLogin();
        await _userRepo.UpdateAsync(user, ct);

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.PasswordHash is null)
            throw new UnauthorizedAccessException("This account uses external login. Please sign in with Google or GitHub.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.TotpCode))
                return new AuthResponse("", "", user.ToProfile(), RequiresTwoFactor: true);

            if (!_totpService.ValidateCode(user.TotpSecret!, request.TotpCode))
                throw new UnauthorizedAccessException("Invalid verification code.");
        }

        user.RecordLogin();
        await _userRepo.UpdateAsync(user, ct);

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = _tokenGenerator.HashToken(refreshToken);
        var stored = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!stored.IsValid())
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        stored.Revoke();
        await _refreshTokenRepo.UpdateAsync(stored, ct);

        var user = await _userRepo.GetByIdAsync(stored.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> OAuthLoginAsync(string provider, string providerUserId, string email, string displayName, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByExternalLoginAsync(provider, providerUserId, ct);

        if (user is null)
        {
            user = await _userRepo.GetByEmailAsync(email, ct);
            if (user is not null)
            {
                user.AddExternalLogin(provider, providerUserId);
                await _userRepo.UpdateAsync(user, ct);
            }
            else
            {
                user = User.Create(_idGen.NewId(), email, displayName);
                user.AddExternalLogin(provider, providerUserId);
                await _userRepo.AddAsync(user, ct);
            }
        }

        user.RecordLogin();
        await _userRepo.UpdateAsync(user, ct);

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = _tokenGenerator.HashToken(refreshToken);
        var stored = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash, ct);

        if (stored is not null && !stored.IsRevoked)
        {
            stored.Revoke();
            await _refreshTokenRepo.UpdateAsync(stored, ct);
        }
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();
        var tokenHash = _tokenGenerator.HashToken(refreshToken);

        var storedToken = RefreshToken.Create(
            _idGen.NewId(),
            tokenHash,
            user.Id,
            DateTime.UtcNow.AddDays(30));

        await _refreshTokenRepo.AddAsync(storedToken, ct);

        return new AuthResponse(accessToken, refreshToken, user.ToProfile());
    }
}
