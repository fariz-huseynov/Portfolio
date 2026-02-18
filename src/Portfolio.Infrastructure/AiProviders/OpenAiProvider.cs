using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.AiProviders;

public class OpenAiProvider : IAiProvider
{
    private readonly OpenAiProviderSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAiProvider> _logger;

    private static readonly string[] AllOperations =
    [
        "GenerateBlogPost", "RewriteText", "GenerateImage", "GenerateSkillDescription",
        "GenerateProjectDescription", "GenerateAboutMe", "GenerateExperienceDescription",
        "GenerateServiceDescription", "SuggestTestimonial"
    ];

    public string ProviderName => "OpenAi";
    public bool IsConfigured => _settings.Enabled && !string.IsNullOrWhiteSpace(_settings.ApiKey);
    public string DefaultModel => _settings.DefaultModel;
    public IReadOnlyList<string> SupportedOperations => AllOperations;

    public OpenAiProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiSettings> settings,
        ILogger<OpenAiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value.OpenAi;
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
            max_tokens = 4096,
            temperature = 0.7
        };

        var response = await client.PostAsJsonAsync("/v1/chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var content = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        var usage = json.GetProperty("usage");

        return new AiTextResult(
            content,
            model,
            usage.GetProperty("prompt_tokens").GetInt32(),
            usage.GetProperty("completion_tokens").GetInt32());
    }

    public async Task<AiImageResult> GenerateImageAsync(
        string prompt, string? size = null, string? style = null, CancellationToken ct = default)
    {
        var client = CreateClient();

        var request = new
        {
            model = _settings.ImageModel,
            prompt,
            n = 1,
            size = size ?? "1024x1024",
            style = style ?? "natural",
            response_format = "b64_json"
        };

        var response = await client.PostAsJsonAsync("/v1/images/generations", request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var b64 = json.GetProperty("data")[0].GetProperty("b64_json").GetString() ?? "";
        var imageBytes = Convert.FromBase64String(b64);

        return new AiImageResult(imageBytes, "image/png", _settings.ImageModel);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress = new Uri(_settings.BaseUrl);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        return client;
    }
}
