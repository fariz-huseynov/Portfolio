using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorDisableDto
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
