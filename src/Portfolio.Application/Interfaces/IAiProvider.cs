namespace Portfolio.Application.Interfaces;

public interface IAiProvider
{
    string ProviderName { get; }
    bool IsConfigured { get; }
    IReadOnlyList<string> SupportedOperations { get; }
    string DefaultModel { get; }

    Task<AiTextResult> GenerateTextAsync(
        string systemPrompt, string userPrompt, string? modelOverride = null, CancellationToken ct = default);

    Task<AiImageResult> GenerateImageAsync(
        string prompt, string? size = null, string? style = null, CancellationToken ct = default);
}

public record AiTextResult(
    string Content,
    string ModelUsed,
    int InputTokens,
    int OutputTokens);

public record AiImageResult(
    byte[] ImageData,
    string ContentType,
    string ModelUsed);
