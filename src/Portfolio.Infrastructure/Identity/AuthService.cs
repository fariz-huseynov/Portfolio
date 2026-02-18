using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.DTOs.Auth;
using Portfolio.Application.Interfaces;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;
    private readonly FrontendSettings _frontendSettings;
    private readonly IPermissionService _permissionService;

    private const string UserNotFound = "User not found.";
    private const string InvalidTwoFactorToken = "Invalid two-factor token.";
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext dbContext,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService,
        IOptions<FrontendSettings> frontendSettings,
        IPermissionService permissionService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
        _frontendSettings = frontendSettings.Value;
        _permissionService = permissionService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // If 2FA is enabled, return a partial response with a 2FA token
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var twoFactorToken = GenerateTwoFactorToken(user);
            return new AuthResponseDto
            {
                RequiresTwoFactor = true,
                TwoFactorToken = twoFactorToken
            };
        }

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken ct = default)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Invalid token.");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.UserId == userId && !t.IsRevoked, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        storedToken.IsRevoked = true;

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException(UserNotFound);

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException(UserNotFound);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Revoke all refresh tokens on password change
        await RevokeAllRefreshTokensAsync(userId, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    // ─── Forgot Password ───────────────────────────────────────

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Always return success to prevent email enumeration
        if (user is null || !user.IsActive)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(dto.Email);

        var resetUrl = $"{_frontendSettings.BaseUrl.TrimEnd('/')}{_frontendSettings.ResetPasswordPath}?email={encodedEmail}&token={encodedToken}";

        var htmlBody = $"""
            <h2>Password Reset Request</h2>
            <p>You requested a password reset for your account.</p>
            <p><a href="{resetUrl}">Click here to reset your password</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>This link will expire in 24 hours.</p>
            """;

        await _emailService.SendAsync(dto.Email, "Password Reset Request", htmlBody, ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordWithTokenDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new InvalidOperationException("Invalid reset request.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Revoke all refresh tokens after password reset
        await RevokeAllRefreshTokensAsync(user.Id, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    // ─── Two-Factor Authentication ─────────────────────────────

    public async Task<TwoFactorSetupResponseDto> SetupTwoFactorAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException(UserNotFound);

        // Reset the authenticator key (generates a new one)
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user)
            ?? throw new InvalidOperationException("Failed to generate authenticator key.");

        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = string.Format(
            AuthenticatorUriFormat,
            UrlEncoder.Default.Encode("Portfolio"),
            UrlEncoder.Default.Encode(email!),
            unformattedKey);

        return new TwoFactorSetupResponseDto
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = authenticatorUri
        };
    }

    public async Task<TwoFactorEnableResponseDto> EnableTwoFactorAsync(Guid userId, TwoFactorVerifyDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException(UserNotFound);

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, dto.Code);

        if (!isValid)
            throw new InvalidOperationException("Invalid verification code.");

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Generate recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)
            ?? throw new InvalidOperationException("Failed to generate recovery codes.");

        return new TwoFactorEnableResponseDto
        {
            RecoveryCodes = recoveryCodes.ToList()
        };
    }

    public async Task DisableTwoFactorAsync(Guid userId, TwoFactorDisableDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException(UserNotFound);

        // Verify password before disabling 2FA
        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Invalid password.");

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
    }

    public async Task<AuthResponseDto> VerifyTwoFactorAsync(TwoFactorLoginDto dto, CancellationToken ct = default)
    {
        var user = await ValidateTwoFactorToken(dto.TwoFactorToken);

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, dto.Code);

        if (!isValid)
            throw new UnauthorizedAccessException("Invalid verification code.");

        return await GenerateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponseDto> RecoveryCodeLoginAsync(TwoFactorRecoveryDto dto, CancellationToken ct = default)
    {
        var user = await ValidateTwoFactorToken(dto.TwoFactorToken);

        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, dto.RecoveryCode);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid recovery code.");

        return await GenerateAuthResponseAsync(user, ct);
    }

    // ─── Private Helpers ───────────────────────────────────────

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetPermissionsForRolesAsync(roles, ct);
        var accessToken = await GenerateAccessTokenAsync(user, roles, permissions, ct);
        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles.ToList(),
                Permissions = permissions.ToList(),
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
            }
        };
    }

    private string GenerateTwoFactorToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("purpose", "2fa"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<ApplicationUser> ValidateTwoFactorToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        ClaimsPrincipal principal;
        try
        {
            principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new UnauthorizedAccessException(InvalidTwoFactorToken);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAccessException("Invalid or expired two-factor token.");
        }

        // Verify this is a 2FA-purpose token
        var purposeClaim = principal.FindFirst("purpose")?.Value;
        if (purposeClaim != "2fa")
            throw new UnauthorizedAccessException(InvalidTwoFactorToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException(InvalidTwoFactorToken);

        var user = await _userManager.FindByIdAsync(userIdClaim)
            ?? throw new UnauthorizedAccessException(UserNotFound);

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        return user;
    }

    private Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles, IReadOnlySet<string> permissions, CancellationToken ct = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false // Allow expired tokens for refresh
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new UnauthorizedAccessException("Invalid token.");

        return principal;
    }

    private async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.IsRevoked = true;
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().TrimEnd();
    }
}
