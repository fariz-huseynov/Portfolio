using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class ExperienceCreateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public bool IsCurrent { get; set; }
    public int SortOrder { get; set; }
}
