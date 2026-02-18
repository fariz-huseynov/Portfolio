namespace Portfolio.Application.Common;

public class AiSettings
{
    public const string SectionName = "AiContent";

    public string DefaultProvider { get; set; } = "OpenAi";
    public OpenAiProviderSettings OpenAi { get; set; } = new();
    public AnthropicProviderSettings Anthropic { get; set; } = new();
    public GeminiProviderSettings Gemini { get; set; } = new();
    public OllamaProviderSettings Ollama { get; set; } = new();
    public int MaxPromptLength { get; set; } = 5000;
    public int GenerationTimeoutSeconds { get; set; } = 120;
}

public class OpenAiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4o";
    public string ImageModel { get; set; } = "dall-e-3";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public bool Enabled { get; set; }
}

public class AnthropicProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "claude-sonnet-4-20250514";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public bool Enabled { get; set; }
}

public class GeminiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gemini-2.0-flash";
    public string ImageModel { get; set; } = "imagen-3";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public bool Enabled { get; set; }
}

public class OllamaProviderSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string DefaultModel { get; set; } = "llama3";
    public bool Enabled { get; set; }
}
