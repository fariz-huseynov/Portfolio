using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Users;

public class ResetPasswordDto
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
