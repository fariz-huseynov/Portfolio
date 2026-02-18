using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Users;

public class CreateUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public List<Guid> RoleIds { get; set; } = [];
}
