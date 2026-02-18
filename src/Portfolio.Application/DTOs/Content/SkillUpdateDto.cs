using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class SkillUpdateDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, 100)]
    public int Proficiency { get; set; }

    [MaxLength(2000)]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
}
