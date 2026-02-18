using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.DTOs.Auth;
using Asp.Versioning;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        var result = await _authService.RefreshTokenAsync(dto, ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        var lastName = User.FindFirstValue(ClaimTypes.Surname);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var permissions = User.FindAll("Permission").Select(c => c.Value).ToArray();
        var isTwoFactorEnabled = User.FindFirstValue("TwoFactorEnabled") == "true";

        return Ok(new
        {
            id = userId,
            email,
            firstName,
            lastName,
            roles,
            permissions,
            isTwoFactorEnabled
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.ChangePasswordAsync(userId, dto, ct);
        return NoContent();
    }

    // ─── Forgot Password ───────────────────────────────────────

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(dto, ct);
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("ForgotPassword")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordWithTokenDto dto, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(dto, ct);
        return Ok(new { message = "Password has been reset successfully." });
    }

    // ─── Two-Factor Authentication ─────────────────────────────

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> SetupTwoFactor(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.SetupTwoFactorAsync(userId, ct);
        return Ok(result);
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<ActionResult<TwoFactorEnableResponseDto>> EnableTwoFactor(
        [FromBody] TwoFactorVerifyDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.EnableTwoFactorAsync(userId, dto, ct);
        return Ok(result);
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(
        [FromBody] TwoFactorDisableDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.DisableTwoFactorAsync(userId, dto, ct);
        return Ok(new { message = "Two-factor authentication has been disabled." });
    }

    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    [EnableRateLimiting("TwoFactorVerify")]
    public async Task<ActionResult<AuthResponseDto>> VerifyTwoFactor(
        [FromBody] TwoFactorLoginDto dto, CancellationToken ct)
    {
        var result = await _authService.VerifyTwoFactorAsync(dto, ct);
        return Ok(result);
    }

    [HttpPost("2fa/recovery")]
    [AllowAnonymous]
    [EnableRateLimiting("TwoFactorVerify")]
    public async Task<ActionResult<AuthResponseDto>> RecoveryLogin(
        [FromBody] TwoFactorRecoveryDto dto, CancellationToken ct)
    {
        var result = await _authService.RecoveryCodeLoginAsync(dto, ct);
        return Ok(result);
    }
}
