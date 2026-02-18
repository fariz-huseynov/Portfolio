using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.AiProviders;

public class GeminiProvider : IAiProvider
{
    private readonly GeminiProviderSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiProvider> _logger;

    private static readonly string[] AllOperations =
    [
        "GenerateBlogPost", "RewriteText", "GenerateImage", "GenerateSkillDescription",
        "GenerateProjectDescription", "GenerateAboutMe", "GenerateExperienceDescription",
        "GenerateServiceDescription", "SuggestTestimonial"
    ];

    public string ProviderName => "Gemini";
    public bool IsConfigured => _settings.Enabled && !string.IsNullOrWhiteSpace(_settings.ApiKey);
    public string DefaultModel => _settings.DefaultModel;
    public IReadOnlyList<string> SupportedOperations => AllOperations;

    public GeminiProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiSettings> settings,
        ILogger<GeminiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value.Gemini;
        _logger = logger;
    }

    public async Task<AiTextResult> GenerateTextAsync(
        string systemPrompt, string userPrompt, string? modelOverride = null, CancellationToken ct = default)
    {
        var client = CreateClient();
        var model = modelOverride ?? _settings.DefaultModel;

        var request = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userPrompt } } }
            },
            generationConfig = new { maxOutputTokens = 4096, temperature = 0.7 }
        };

        var url = $"/v1beta/models/{model}:generateContent?key={_settings.ApiKey}";
        var response = await client.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var content = json.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? "";

        var inputTokens = 0;
        var outputTokens = 0;
        if (json.TryGetProperty("usageMetadata", out var usage))
        {
            inputTokens = usage.TryGetProperty("promptTokenCount", out var inp) ? inp.GetInt32() : 0;
            outputTokens = usage.TryGetProperty("candidatesTokenCount", out var outp) ? outp.GetInt32() : 0;
        }

        return new AiTextResult(content, model, inputTokens, outputTokens);
    }

    public async Task<AiImageResult> GenerateImageAsync(
        string prompt, string? size = null, string? style = null, CancellationToken ct = default)
    {
        var client = CreateClient();
        var model = _settings.ImageModel;

        var request = new
        {
            instances = new[] { new { prompt } },
            parameters = new { sampleCount = 1, aspectRatio = size ?? "1:1" }
        };

        var url = $"/v1beta/models/{model}:predict?key={_settings.ApiKey}";
        var response = await client.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var b64 = json.GetProperty("predictions")[0]
            .GetProperty("bytesBase64Encoded").GetString() ?? "";
        var imageBytes = Convert.FromBase64String(b64);

        return new AiImageResult(imageBytes, "image/png", model);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress = new Uri(_settings.BaseUrl);
        return client;
    }
}
