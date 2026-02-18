using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.AiProviders;

public class AnthropicProvider : IAiProvider
{
    private readonly AnthropicProviderSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AnthropicProvider> _logger;

    private static readonly string[] TextOperations =
    [
        "GenerateBlogPost", "RewriteText", "GenerateSkillDescription",
        "GenerateProjectDescription", "GenerateAboutMe", "GenerateExperienceDescription",
        "GenerateServiceDescription", "SuggestTestimonial"
    ];

    public string ProviderName => "Anthropic";
    public bool IsConfigured => _settings.Enabled && !string.IsNullOrWhiteSpace(_settings.ApiKey);
    public string DefaultModel => _settings.DefaultModel;
    public IReadOnlyList<string> SupportedOperations => TextOperations;

    public AnthropicProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiSettings> settings,
        ILogger<AnthropicProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value.Anthropic;
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
            max_tokens = 4096,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var response = await client.PostAsJsonAsync("/v1/messages", request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var content = json.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
        var usage = json.GetProperty("usage");

        return new AiTextResult(
            content,
            model,
            usage.GetProperty("input_tokens").GetInt32(),
            usage.GetProperty("output_tokens").GetInt32());
    }

    public Task<AiImageResult> GenerateImageAsync(
        string prompt, string? size = null, string? style = null, CancellationToken ct = default)
    {
        throw new NotSupportedException("Anthropic does not support image generation. Use OpenAI or Gemini instead.");
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress = new Uri(_settings.BaseUrl);
        client.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        return client;
    }
}
