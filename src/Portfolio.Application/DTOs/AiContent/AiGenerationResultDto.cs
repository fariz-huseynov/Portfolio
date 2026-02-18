namespace Portfolio.Application.DTOs.AiContent;

public class AiGenerationResultDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? ResultContent { get; set; }
    public string? ResultImageUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ModelName { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public double? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
