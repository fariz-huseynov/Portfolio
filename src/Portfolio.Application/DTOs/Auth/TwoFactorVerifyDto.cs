using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorVerifyDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
}
