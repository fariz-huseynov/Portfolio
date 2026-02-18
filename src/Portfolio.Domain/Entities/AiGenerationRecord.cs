namespace Portfolio.Domain.Entities;

public class AiGenerationRecord : BaseEntity
{
    public AiProvider Provider { get; set; }
    public AiOperationType OperationType { get; set; }
    public AiGenerationStatus Status { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public string? ResultContent { get; set; }
    public string? ResultImageUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ModelName { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public double? DurationSeconds { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}
