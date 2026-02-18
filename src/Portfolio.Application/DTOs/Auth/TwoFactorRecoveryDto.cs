using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorRecoveryDto
{
    [Required]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required]
    public string RecoveryCode { get; set; } = string.Empty;
}
