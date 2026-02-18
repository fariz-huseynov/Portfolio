using Portfolio.Application.DTOs.Auth;

namespace Portfolio.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default);

    // Forgot Password
    Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordWithTokenDto dto, CancellationToken ct = default);

    // Two-Factor Authentication
    Task<TwoFactorSetupResponseDto> SetupTwoFactorAsync(Guid userId, CancellationToken ct = default);
    Task<TwoFactorEnableResponseDto> EnableTwoFactorAsync(Guid userId, TwoFactorVerifyDto dto, CancellationToken ct = default);
    Task DisableTwoFactorAsync(Guid userId, TwoFactorDisableDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> VerifyTwoFactorAsync(TwoFactorLoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RecoveryCodeLoginAsync(TwoFactorRecoveryDto dto, CancellationToken ct = default);
}
