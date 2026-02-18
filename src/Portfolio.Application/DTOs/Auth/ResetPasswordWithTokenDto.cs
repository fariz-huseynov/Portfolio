using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Auth;

public class ResetPasswordWithTokenDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
