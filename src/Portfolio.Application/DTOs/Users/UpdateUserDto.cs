using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Users;

public class UpdateUserDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}
