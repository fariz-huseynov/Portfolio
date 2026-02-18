using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs;

public class BlogPostCreateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}
