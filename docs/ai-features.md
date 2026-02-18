# AI Content Generation

The Portfolio CMS includes AI-powered content generation supporting multiple providers. Generate blog posts, rewrite text, create images, and auto-generate descriptions for skills, projects, experiences, and more.

## Supported Providers

| Provider | Text Generation | Image Generation | API Key Required | Default Model |
|----------|:-:|:-:|:-:|---------------|
| **OpenAI** | Yes | Yes | Yes | `gpt-4o` / `dall-e-3` |
| **Anthropic** | Yes | No | Yes | `claude-sonnet-4-20250514` |
| **Google Gemini** | Yes | Yes | Yes | `gemini-2.0-flash` / `imagen-3` |
| **Ollama** | Yes | No | No (local) | `llama3` |

## Configuration

Enable providers by setting their API key and `Enabled: true` in `appsettings.json`:

```json
{
  "AiContent": {
    "DefaultProvider": "OpenAi",
    "MaxPromptLength": 5000,
    "GenerationTimeoutSeconds": 120,
    "OpenAi": {
      "ApiKey": "sk-...",
      "DefaultModel": "gpt-4o",
      "ImageModel": "dall-e-3",
      "BaseUrl": "https://api.openai.com/v1",
      "Enabled": true
    },
    "Anthropic": {
      "ApiKey": "sk-ant-...",
      "DefaultModel": "claude-sonnet-4-20250514",
      "BaseUrl": "https://api.anthropic.com/v1",
      "Enabled": true
    },
    "Gemini": {
      "ApiKey": "AIza...",
      "DefaultModel": "gemini-2.0-flash",
      "ImageModel": "imagen-3",
      "BaseUrl": "https://generativelanguage.googleapis.com/v1beta",
      "Enabled": true
    },
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "DefaultModel": "llama3",
      "Enabled": true
    }
  }
}
```

For Docker, use environment variable overrides:

```yaml
environment:
  - AiContent__OpenAi__ApiKey=sk-...
  - AiContent__OpenAi__Enabled=true
```

## Operation Types

Each operation uses a specialized system prompt to produce targeted output:

| Operation | Description | Example Use |
|-----------|-------------|-------------|
| `GenerateBlogPost` | Full markdown blog post with title, sections, and conclusion | Blog creation |
| `RewriteText` | Rewrite existing text with specific instructions | Editing blog posts |
| `GenerateImage` | Create an image from a text description | Blog featured images |
| `GenerateSkillDescription` | 1-2 sentence skill description | Skills section |
| `GenerateProjectDescription` | Detailed project description with technologies | Portfolio projects |
| `GenerateAboutMe` | Personal branding "About Me" text | About section |
| `GenerateExperienceDescription` | Job experience with bullet points | Experience section |
| `GenerateServiceDescription` | 2-3 sentence service description | Services section |
| `SuggestTestimonial` | Realistic client testimonial text | Testimonials section |

## API Usage

### Generate Text

```bash
curl -X POST http://localhost:5001/api/v1/admin/ai/generate-text \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "GenerateBlogPost",
    "prompt": "Write about microservices vs monoliths in 2026",
    "additionalContext": "Target audience: junior developers",
    "preferredProvider": "OpenAi",
    "preferredModel": "gpt-4o"
  }'
```

**Response:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "provider": "OpenAi",
  "operationType": "GenerateBlogPost",
  "status": "Completed",
  "prompt": "Write about microservices vs monoliths in 2026",
  "resultContent": "# Microservices vs Monoliths in 2026\n\n...",
  "modelName": "gpt-4o",
  "inputTokens": 180,
  "outputTokens": 1500,
  "durationSeconds": 4.2,
  "completedAt": "2026-02-18T12:00:04Z"
}
```

### Rewrite Text

```bash
curl -X POST http://localhost:5001/api/v1/admin/ai/rewrite-text \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "originalText": "I made a website using React.",
    "instructions": "Make it more professional and detailed",
    "preferredProvider": "Anthropic"
  }'
```

### Generate Image

```bash
curl -X POST http://localhost:5001/api/v1/admin/ai/generate-image \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "A modern developer workspace with multiple monitors",
    "size": "1024x1024",
    "style": "natural",
    "preferredProvider": "OpenAi"
  }'
```

**Response:**

```json
{
  "id": "...",
  "provider": "OpenAi",
  "operationType": "GenerateImage",
  "status": "Completed",
  "resultImageUrl": "/uploads/2026/02/ai-generated-abc123.png",
  "modelName": "dall-e-3",
  "completedAt": "2026-02-18T12:00:10Z"
}
```

### View Generation History

```bash
# Get all generations (paginated)
curl http://localhost:5001/api/v1/admin/ai/generations?pageNumber=1&pageSize=10 \
  -H "Authorization: Bearer <token>"

# Get a specific generation
curl http://localhost:5001/api/v1/admin/ai/generations/{id} \
  -H "Authorization: Bearer <token>"
```

### Check Available Providers

```bash
curl http://localhost:5001/api/v1/admin/ai/providers \
  -H "Authorization: Bearer <token>"
```

**Response:**

```json
[
  {
    "provider": "OpenAi",
    "isConfigured": true,
    "defaultModel": "gpt-4o",
    "supportedOperations": [
      "GenerateBlogPost", "RewriteText", "GenerateImage",
      "GenerateSkillDescription", "GenerateProjectDescription",
      "GenerateAboutMe", "GenerateExperienceDescription",
      "GenerateServiceDescription", "SuggestTestimonial"
    ]
  },
  {
    "provider": "Anthropic",
    "isConfigured": false,
    "defaultModel": "claude-sonnet-4-20250514",
    "supportedOperations": [
      "GenerateBlogPost", "RewriteText",
      "GenerateSkillDescription", "GenerateProjectDescription",
      "GenerateAboutMe", "GenerateExperienceDescription",
      "GenerateServiceDescription", "SuggestTestimonial"
    ]
  }
]
```

## Architecture

### Strategy Pattern

All providers implement the `IAiProvider` interface:

```
IAiProvider (interface)
├── OpenAiProvider    → GPT-4o, DALL-E 3
├── AnthropicProvider → Claude (text only)
├── GeminiProvider    → Gemini, Imagen 3
└── OllamaProvider   → Local models (text only)
```

Providers are registered in DI as `IEnumerable<IAiProvider>`. The `AiContentService` orchestrator resolves the correct provider by name at runtime.

### Request Flow

```
Client → AdminAiController → AiContentService → IAiProvider
                                    │
                                    ├── Creates AiGenerationRecord (status: Pending)
                                    ├── Resolves provider by name
                                    ├── Builds prompt from AiPromptTemplates
                                    ├── Calls provider.GenerateTextAsync/GenerateImageAsync
                                    ├── Saves result (status: Completed/Failed)
                                    └── Returns AiGenerationResultDto
```

### Rate Limiting

AI generation endpoints are protected by the `AiGeneration` rate limiter (10 requests/minute in production). This prevents excessive API costs.

### Generation Records

Every AI generation is persisted in the `AiGenerationRecords` table with:

- Provider and model used
- Full prompt and system prompt
- Result content or image URL
- Token usage (input/output)
- Duration in seconds
- Status (Completed/Failed) with error messages

## Adding a New Provider

1. **Create provider class** in `src/Portfolio.Infrastructure/AiProviders/`:

```csharp
public class NewProvider : IAiProvider
{
    public string ProviderName => "NewProvider";
    public bool IsConfigured => _settings.Enabled && !string.IsNullOrEmpty(_settings.ApiKey);
    // ... implement GenerateTextAsync, GenerateImageAsync
}
```

2. **Add settings** in `AiSettings.cs`:

```csharp
public NewProviderSettings NewProvider { get; set; } = new();
```

3. **Add enum value** in `Domain/Entities/AiProvider.cs`:

```csharp
NewProvider = 4
```

4. **Register in DI** (`Infrastructure/DependencyInjection.cs`):

```csharp
services.AddHttpClient("NewProvider", client => { /* configure */ });
services.AddScoped<IAiProvider, NewProvider>();
```

5. **Add configuration** in `appsettings.json` under `AiContent:NewProvider`.

## Using Ollama (Local AI)

Ollama runs AI models locally without API keys:

```bash
# Install Ollama
curl -fsSL https://ollama.com/install.sh | sh

# Pull a model
ollama pull llama3

# Ollama runs on http://localhost:11434 by default
```

Enable in config:

```json
{
  "AiContent": {
    "DefaultProvider": "Ollama",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "DefaultModel": "llama3",
      "Enabled": true
    }
  }
}
```

When running in Docker, set `BaseUrl` to `http://host.docker.internal:11434` so the container can reach Ollama on the host machine.
