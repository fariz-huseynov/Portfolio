using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.AiContent;

public class AiRewriteTextRequestDto
{
    [Required]
    public string OriginalText { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Instructions { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PreferredProvider { get; set; }

    [MaxLength(100)]
    public string? PreferredModel { get; set; }
}
