using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class SocialLinkCreateDto
{
    [Required, MaxLength(100)]
    public string Platform { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}
