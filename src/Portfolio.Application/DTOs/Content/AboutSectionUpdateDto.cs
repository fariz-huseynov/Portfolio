using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class AboutSectionUpdateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(2000)]
    public string? ResumeUrl { get; set; }
}
