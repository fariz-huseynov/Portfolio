namespace Portfolio.Application.DTOs.AiContent;

public class AiProviderStatusDto
{
    public string Provider { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public string? DefaultModel { get; set; }
    public IReadOnlyList<string> SupportedOperations { get; set; } = [];
}
