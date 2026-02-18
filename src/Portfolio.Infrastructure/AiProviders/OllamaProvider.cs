using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.AiProviders;

public class OllamaProvider : IAiProvider
{
    private readonly OllamaProviderSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OllamaProvider> _logger;

    private static readonly string[] TextOperations =
    [
        "GenerateBlogPost", "RewriteText", "GenerateSkillDescription",
        "GenerateProjectDescription", "GenerateAboutMe", "GenerateExperienceDescription",
        "GenerateServiceDescription", "SuggestTestimonial"
    ];

    public string ProviderName => "Ollama";
    public bool IsConfigured => _settings.Enabled;
    public string DefaultModel => _settings.DefaultModel;
    public IReadOnlyList<string> SupportedOperations => TextOperations;

    public OllamaProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiSettings> settings,
        ILogger<OllamaProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value.Ollama;
        _logger = logger;
    }

    public async Task<AiTextResult> GenerateTextAsync(
        string systemPrompt, string userPrompt, string? modelOverride = null, CancellationToken ct = default)
    {
        var client = CreateClient();
        var model = modelOverride ?? _settings.DefaultModel;

        var request = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false
        };

        var response = await client.PostAsJsonAsync("/api/chat", request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var content = json.GetProperty("message").GetProperty("content").GetString() ?? "";

        var inputTokens = 0;
        var outputTokens = 0;
        if (json.TryGetProperty("prompt_eval_count", out var inp))
            inputTokens = inp.GetInt32();
        if (json.TryGetProperty("eval_count", out var outp))
            outputTokens = outp.GetInt32();

        return new AiTextResult(content, model, inputTokens, outputTokens);
    }

    public Task<AiImageResult> GenerateImageAsync(
        string prompt, string? size = null, string? style = null, CancellationToken ct = default)
    {
        throw new NotSupportedException("Ollama does not support image generation. Use OpenAI or Gemini instead.");
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress = new Uri(_settings.BaseUrl);
        return client;
    }
}
