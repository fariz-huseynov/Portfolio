using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.AiContent;

public class AiGenerateTextRequestDto
{
    [Required, MaxLength(100)]
    public string OperationType { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Prompt { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? AdditionalContext { get; set; }

    [MaxLength(50)]
    public string? PreferredProvider { get; set; }

    [MaxLength(100)]
    public string? PreferredModel { get; set; }
}
