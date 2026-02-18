using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Roles;

public class CreateRoleDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<Guid> PermissionIds { get; set; } = [];
}
