using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs;

public class ProjectUpdateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? GitHubUrl { get; set; }

    [Required]
    public string TechStack { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
