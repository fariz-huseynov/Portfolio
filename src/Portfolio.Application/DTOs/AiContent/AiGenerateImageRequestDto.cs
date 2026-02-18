using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.AiContent;

public class AiGenerateImageRequestDto
{
    [Required, MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Size { get; set; }

    [MaxLength(50)]
    public string? Style { get; set; }

    [MaxLength(50)]
    public string? PreferredProvider { get; set; }
}
