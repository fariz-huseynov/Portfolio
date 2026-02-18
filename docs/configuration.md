# Configuration

All configuration uses the standard ASP.NET Core configuration system. Values can be set in `appsettings.json`, `appsettings.Development.json`, environment variables, or Docker Compose overrides.

Environment variables use `__` (double underscore) notation: `Jwt__Secret` maps to `Jwt:Secret` in JSON.

---

## Connection Strings

| Key | Description | Default (dev) |
|-----|-------------|---------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection | `Server=localhost;Database=Portfolio_Dev;User Id=sa;Password=Dev@12345;TrustServerCertificate=True;MultipleActiveResultSets=true` |
| `ConnectionStrings:Redis` | Redis connection | `localhost:6379` |

**Production**: Use Azure SQL or a managed SQL Server instance. Always set `TrustServerCertificate=False` and use proper certificates.

---

## JWT Authentication

| Key | Description | Default |
|-----|-------------|---------|
| `Jwt:Secret` | HMAC signing key (min 32 chars) | Dev key (change in production!) |
| `Jwt:Issuer` | Token issuer claim | `Portfolio` |
| `Jwt:Audience` | Token audience claim | `Portfolio` |
| `Jwt:AccessTokenExpirationMinutes` | Access token lifetime | `60` |
| `Jwt:RefreshTokenExpirationDays` | Refresh token lifetime | `7` |

**Production**: Generate a cryptographically random secret (64+ characters). Never commit production secrets to source control.

---

## Database Seeding

| Key | Description | Default |
|-----|-------------|---------|
| `Seed:AdminEmail` | Initial admin account email | `admin@portfolio.dev` |
| `Seed:AdminPassword` | Initial admin account password | `Admin@123456` |

The seeder runs on startup and creates the admin user only if no users exist. Change credentials in production.

---

## CORS

| Key | Description | Default |
|-----|-------------|---------|
| `Cors:AllowedOrigins` | Array of allowed frontend origins | `["http://localhost:3000"]` |

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://yourdomain.com",
      "https://www.yourdomain.com"
    ]
  }
}
```

---

## Caching

Controls the hybrid L1 (in-memory) + L2 (Redis) cache TTLs.

| Key | Description | Default (prod) | Default (dev) |
|-----|-------------|----------------|---------------|
| `Caching:SiteContentMinutes` | Site settings, hero, about, etc. | `10` | `5` |
| `Caching:BlogPostMinutes` | Individual blog posts | `30` | `5` |
| `Caching:ProjectMinutes` | Individual projects | `30` | `5` |
| `Caching:PublishedListMinutes` | Published post/project lists | `15` | `5` |
| `Caching:IpRulesMinutes` | IP allow/block rules | `5` | `2` |
| `Caching:PermissionsMinutes` | Role-to-permission mappings | `5` | `2` |

Cache is automatically invalidated when content is created, updated, or deleted.

---

## Email

| Key | Description | Default |
|-----|-------------|---------|
| `Email:Host` | SMTP server hostname | `localhost` |
| `Email:Port` | SMTP port | `587` (prod) / `25` (dev) |
| `Email:FromEmail` | Sender email address | `noreply@localhost` |
| `Email:FromName` | Sender display name | `Portfolio` |
| `Email:Username` | SMTP username | (empty) |
| `Email:Password` | SMTP password | (empty) |
| `Email:UseSsl` | Use TLS/SSL | `true` (prod) / `false` (dev) |
| `Email:Enabled` | Enable email sending | `true` (prod) / `false` (dev) |

Used for password reset emails. When `Enabled` is `false`, password reset tokens are logged instead of emailed.

---

## Frontend

| Key | Description | Default |
|-----|-------------|---------|
| `Frontend:BaseUrl` | Frontend app URL (used in email links) | `http://localhost:3000` |
| `Frontend:ResetPasswordPath` | Password reset page path | `/reset-password` |

Password reset emails contain a link: `{BaseUrl}{ResetPasswordPath}?token=...&email=...`

---

## File Storage

| Key | Description | Default |
|-----|-------------|---------|
| `FileStorage:UploadDirectory` | Upload folder (relative to wwwroot) | `uploads` |
| `FileStorage:MaxFileSizeBytes` | Max upload size | `10485760` (10 MB) |
| `FileStorage:AllowedImageExtensions` | Allowed image types | `.jpg`, `.jpeg`, `.png`, `.gif` |
| `FileStorage:AllowedDocumentExtensions` | Allowed document types | `.pdf` |

Files are validated by extension, MIME type, and magic bytes.

---

## AI Content Generation

See [AI Features](ai-features.md) for detailed usage. All settings are under the `AiContent` section.

| Key | Description | Default |
|-----|-------------|---------|
| `AiContent:DefaultProvider` | Fallback provider when none specified | `OpenAi` |
| `AiContent:MaxPromptLength` | Maximum prompt character length | `5000` |
| `AiContent:GenerationTimeoutSeconds` | HTTP client timeout per provider | `120` |

### Per-Provider Settings

Each provider has its own subsection:

**OpenAI** (`AiContent:OpenAi`):

| Key | Description | Default |
|-----|-------------|---------|
| `ApiKey` | OpenAI API key | (empty — must set) |
| `DefaultModel` | Text generation model | `gpt-4o` (prod) / `gpt-4o-mini` (dev) |
| `ImageModel` | Image generation model | `dall-e-3` |
| `BaseUrl` | API base URL | `https://api.openai.com/v1` |
| `Enabled` | Enable this provider | `false` |

**Anthropic** (`AiContent:Anthropic`):

| Key | Description | Default |
|-----|-------------|---------|
| `ApiKey` | Anthropic API key | (empty) |
| `DefaultModel` | Text generation model | `claude-sonnet-4-20250514` |
| `BaseUrl` | API base URL | `https://api.anthropic.com/v1` |
| `Enabled` | Enable this provider | `false` |

**Google Gemini** (`AiContent:Gemini`):

| Key | Description | Default |
|-----|-------------|---------|
| `ApiKey` | Google AI API key | (empty) |
| `DefaultModel` | Text generation model | `gemini-2.0-flash` |
| `ImageModel` | Image generation model | `imagen-3` |
| `BaseUrl` | API base URL | `https://generativelanguage.googleapis.com/v1beta` |
| `Enabled` | Enable this provider | `false` |

**Ollama** (`AiContent:Ollama`):

| Key | Description | Default |
|-----|-------------|---------|
| `BaseUrl` | Ollama server URL | `http://localhost:11434` |
| `DefaultModel` | Default model name | `llama3` |
| `Enabled` | Enable this provider | `false` |

---

## CAPTCHA

| Key | Description | Default |
|-----|-------------|---------|
| `CaptchaOptions:CaptchaType` | Captcha style | `DEFAULT` |
| `CaptchaOptions:CodeLength` | Number of characters | `4` |
| `CaptchaOptions:ExpirySeconds` | Captcha validity period | `60` |
| `CaptchaOptions:IgnoreCase` | Case-insensitive validation | `true` |
| `CaptchaOptions:ImageOption:Width` | Image width | `150` |
| `CaptchaOptions:ImageOption:Height` | Image height | `50` |

---

## Rate Limiting

All rate limits are configurable via the `RateLimiting` section:

| Key | Limit (prod) | Window | Description |
|-----|-------------|--------|-------------|
| `RateLimiting:Auth:PermitLimit` | `10` | 1 min | Login, refresh, 2FA |
| `RateLimiting:ForgotPassword:PermitLimit` | `3` | 15 min | Password reset |
| `RateLimiting:TwoFactorVerify:PermitLimit` | `5` | 1 min | 2FA verification |
| `RateLimiting:LeadSubmit:PermitLimit` | `5` | 1 min | Contact form |
| `RateLimiting:AiGeneration:PermitLimit` | `10` | 1 min | AI generation |
| `RateLimiting:PublicApi:PermitLimit` | `30` | 1 min | Public endpoints |
| `RateLimiting:Global:PermitLimit` | `100` | 1 min | Per-IP global |

Each policy has `PermitLimit` and `WindowMinutes` sub-keys. Development defaults are much higher (100-10000) for comfortable testing.

---

## Security

| Key | Description | Default |
|-----|-------------|---------|
| `Security:MaxRequestBodySizeBytes` | Kestrel max body size | `10485760` (10 MB) |
| `Security:JwtClockSkewMinutes` | JWT clock skew tolerance | `1` |
| `Security:SecurityMonitoringWindowMinutes` | Threat detection window | `15` |
| `Security:SecurityMonitoringThreshold` | Failed attempts before flagging | `10` |

---

## Serilog

Structured logging with Serilog. Logs go to stdout (Console sink) which is picked up by the OpenTelemetry collector.

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

**Production**: Consider adding a file sink or external log aggregation service. The Console sink is always recommended as it enables Docker log collection.

---

## Production Checklist

Before deploying to production, ensure these are changed from defaults:

- [ ] `Jwt:Secret` — use a cryptographically random 64+ character string
- [ ] `Seed:AdminPassword` — use a strong password
- [ ] `ConnectionStrings:DefaultConnection` — use production database credentials
- [ ] `ConnectionStrings:Redis` — use production Redis with authentication
- [ ] `Cors:AllowedOrigins` — set to your actual frontend domain(s)
- [ ] `Email:*` — configure a real SMTP provider
- [ ] `Frontend:BaseUrl` — set to your production frontend URL
- [ ] `AiContent:*:ApiKey` — set API keys only for providers you intend to use
- [ ] All `RateLimiting` values — review for production traffic patterns
