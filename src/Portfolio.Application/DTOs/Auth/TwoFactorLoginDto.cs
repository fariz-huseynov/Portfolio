using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorLoginDto
{
    [Required]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
