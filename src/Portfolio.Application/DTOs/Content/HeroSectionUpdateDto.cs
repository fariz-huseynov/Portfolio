using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class HeroSectionUpdateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Subtitle { get; set; }

    [MaxLength(2000)]
    public string? BackgroundImageUrl { get; set; }

    [MaxLength(100)]
    public string? CtaText { get; set; }

    [MaxLength(2000)]
    public string? CtaUrl { get; set; }
}
