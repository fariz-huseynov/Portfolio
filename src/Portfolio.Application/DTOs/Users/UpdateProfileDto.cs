using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Users;

public class UpdateProfileDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? AvatarUrl { get; set; }
}
