# Portfolio

A production-ready personal portfolio and CMS backend API built with .NET 10, Clean Architecture, and a full-featured admin API. Designed as an open-source starting point for developers who want a solid backend for their personal website.

Developed by **Software Engineer Fariz Huseynov**.

## Quick Start

```bash
docker compose up -d
```

That's it. The API is now running at **http://localhost:5001**.

- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health
- **Aspire Dashboard** (traces/logs): http://localhost:18888

### Default Credentials

| Email | Password | Role |
|-------|----------|------|
| admin@portfolio.dev | Admin@123456 | SuperAdmin |

### Test It

```bash
# Login
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@portfolio.dev","password":"Admin@123456"}'

# Use the accessToken from the response
curl http://localhost:5001/api/v1/admin/users \
  -H "Authorization: Bearer <your-token>"
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 10 |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API) |
| Database | SQL Server (Azure SQL Edge for ARM/Docker) |
| Cache | Redis (L1 in-memory + L2 distributed) |
| Auth | JWT with refresh tokens, TOTP 2FA |
| Authorization | Permission-based RBAC (27 permissions) |
| AI | Multi-provider content generation (OpenAI, Anthropic, Gemini, Ollama) |
| Observability | OpenTelemetry + Aspire Dashboard |
| Logging | Serilog (structured, console) |
| API Docs | Swagger / OpenAPI |
| Real-time | SignalR |
| Containerization | Docker with multi-stage builds |

## Architecture

```
Portfolio/
├── src/
│   ├── Portfolio.Domain/             # Entities, interfaces, constants
│   ├── Portfolio.Application/        # DTOs, service interfaces, business logic
│   ├── Portfolio.Infrastructure/     # EF Core, Identity, repositories, caching, AI providers
│   ├── Portfolio.Api/                # Controllers, middleware, Program.cs
│   └── Portfolio.ServiceDefaults/    # OpenTelemetry, health checks, service discovery
├── tests/
│   ├── Portfolio.Api.Tests/          # Integration tests
│   ├── Portfolio.Application.Tests/  # Unit tests (services)
│   └── Portfolio.Infrastructure.Tests/ # Unit tests (infrastructure)
├── docs/                             # Detailed documentation
├── docker-compose.yml                # Full dev environment (SQL + Redis + Aspire + API)
├── Dockerfile                        # Multi-stage production build
└── Portfolio.sln
```

### Clean Architecture Layers

```
Domain (entities, repository interfaces)
    ↑
Application (DTOs, service interfaces, business logic)
    ↑
Infrastructure (EF Core, Identity, Redis cache, AI providers, email, file storage)
    ↑
API (controllers, middleware, auth pipeline, SignalR hubs)
```

## Documentation

Detailed documentation is available in the [`docs/`](docs/) folder:

| Document | Description |
|----------|-------------|
| [Setup Guide](docs/setup.md) | Get the project running locally or with Docker |
| [Architecture](docs/architecture.md) | Clean Architecture layers, design patterns |
| [API Reference](docs/api-reference.md) | All 109 endpoints with request/response examples |
| [AI Features](docs/ai-features.md) | AI content generation: providers, configuration, usage |
| [Configuration](docs/configuration.md) | All configuration sections explained |
| [Testing](docs/testing.md) | Test strategy, running tests, test infrastructure |
| [Deployment](docs/deployment.md) | Production deployment guide |

## Docker Services

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| api | Built from Dockerfile | 5001 | Portfolio API |
| sqlserver | Azure SQL Edge | 1433 | Database |
| redis | Redis 7 Alpine | 6379 | Distributed cache + output cache |
| aspire-dashboard | Aspire Dashboard 9.1 | 18888 | Traces, metrics, logs |

## Local Development (without Docker)

If you prefer running outside Docker:

```bash
# 1. Start SQL Server and Redis (via Docker or locally)
docker run -d --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Dev@12345" -p 1433:1433 mcr.microsoft.com/azure-sql-edge:latest
docker run -d --name redis -p 6379:6379 redis:7-alpine

# 2. Run the API
cd src/Portfolio.Api
dotnet run
```

The API starts on **http://localhost:5001**. All configuration is in `appsettings.Development.json` (no user-secrets needed).

## API Endpoints

### Public (Anonymous)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/site/settings` | Site settings |
| GET | `/api/v1/site/hero` | Hero section |
| GET | `/api/v1/site/about` | About section |
| GET | `/api/v1/site/skills` | Skills list |
| GET | `/api/v1/site/experiences` | Experiences |
| GET | `/api/v1/site/services` | Services |
| GET | `/api/v1/site/testimonials` | Testimonials |
| GET | `/api/v1/site/social-links` | Social links |
| GET | `/api/v1/site/menu` | Menu tree |
| GET | `/api/v1/portfolio/projects` | Published projects |
| GET | `/api/v1/portfolio/projects/paged` | Projects (paginated) |
| GET | `/api/v1/portfolio/projects/{slug}` | Project by slug |
| GET | `/api/v1/portfolio/blogs` | Published blog posts |
| GET | `/api/v1/portfolio/blogs/paged` | Blog posts (paginated) |
| GET | `/api/v1/portfolio/blogs/{slug}` | Blog post by slug |
| POST | `/api/v1/leads/submit` | Submit contact form |
| GET | `/api/v1/captcha/generate` | Generate CAPTCHA |
| GET | `/api/v1/captcha/image` | Get CAPTCHA image |
| GET | `/health` | Health check |

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Login (returns JWT) |
| POST | `/api/v1/auth/refresh` | Refresh token |
| POST | `/api/v1/auth/logout` | Logout |
| GET | `/api/v1/auth/me` | Current user info |
| POST | `/api/v1/auth/change-password` | Change password |
| POST | `/api/v1/auth/forgot-password` | Request password reset |
| POST | `/api/v1/auth/reset-password` | Reset with token |
| POST | `/api/v1/auth/2fa/setup` | Setup TOTP 2FA |
| POST | `/api/v1/auth/2fa/enable` | Enable 2FA |
| POST | `/api/v1/auth/2fa/disable` | Disable 2FA |
| POST | `/api/v1/auth/2fa/verify` | Verify TOTP code |
| POST | `/api/v1/auth/2fa/recovery` | Use recovery code |

### Admin (JWT + Permission Required)

| Resource | Endpoints | Permissions |
|----------|-----------|-------------|
| Users | CRUD + reset password | Users.* |
| Roles | CRUD + permissions | Users.* |
| Blog Posts | CRUD + publish/unpublish | Blogs.* |
| Projects | CRUD | Projects.* |
| Leads | List + mark read | Leads.* |
| Site Content | Hero, About, Skills, Experiences, Services, Testimonials, Social Links, Menu | SiteContent.* |
| Settings | View + update | Settings.* |
| IP Rules | CRUD | Security.* |
| Files | Upload, list, delete | Files.Manage |
| AI Content | Generate text, rewrite, generate images, history, providers | AiContent.* |
| Profile | View + update + avatar | Any authenticated user |

For the full 109-endpoint reference with request/response examples, see [docs/api-reference.md](docs/api-reference.md).

## AI Content Generation

Multi-provider AI content generation supporting text and image creation for your portfolio content.

| Provider | Text | Image | Default Model |
|----------|:----:|:-----:|---------------|
| OpenAI | Yes | Yes | gpt-4o / dall-e-3 |
| Anthropic | Yes | No | claude-sonnet-4-20250514 |
| Google Gemini | Yes | Yes | gemini-2.0-flash / imagen-3 |
| Ollama (local) | Yes | No | llama3 |

Supports 9 operation types: blog posts, text rewriting, image generation, skill/project/about/experience/service descriptions, and testimonial suggestions.

See [docs/ai-features.md](docs/ai-features.md) for setup and usage.

## Security

### Rate Limiting

| Policy | Limit | Scope |
|--------|-------|-------|
| Auth | 10/min/IP | Login, refresh |
| ForgotPassword | 3/15min/IP | Password reset |
| TwoFactorVerify | 5/min/IP | 2FA verification |
| LeadSubmit | 5/min/IP | Contact form |
| AiGeneration | 10/min/IP | AI content generation |
| PublicApi | 30/min/IP | Public endpoints |
| Global | 100/min/IP | All endpoints |

### Other Security Features

- **JWT authentication** with short-lived access tokens and refresh tokens
- **Permission-based authorization** with 27 granular permissions
- **TOTP two-factor authentication** with recovery codes
- **CAPTCHA protection** on lead submission (Lazy.Captcha.Core)
- **HTML sanitization** on all rich-text content (XSS prevention)
- **Security headers** (HSTS, X-Frame-Options, CSP, etc.)
- **IP allow/block rules** with middleware enforcement
- **File upload validation** (extension, MIME, magic bytes, size, content scanning)
- **Response compression** (Brotli + Gzip)
- **Output caching** with Redis backend (auto-eviction on content changes)
- **API idempotency** via `Idempotency-Key` header on POST requests
- **Self-action prevention** (admins can't delete/disable their own account)

## Caching

Two-tier hybrid caching strategy:

- **L1**: In-memory cache (fast, per-instance)
- **L2**: Redis distributed cache (shared, survives restarts)
- **Output Cache**: Redis-backed HTTP response caching for public endpoints (5-minute TTL)

## Observability

OpenTelemetry exports traces, metrics, and logs to the Aspire Dashboard:

- **Traces**: HTTP requests, database queries, Redis operations
- **Metrics**: ASP.NET Core, HTTP client, .NET runtime
- **Logs**: Structured Serilog output with OpenTelemetry integration

View everything at **http://localhost:18888** when running via Docker Compose.

## Database

- **Provider**: SQL Server (Azure SQL Edge in Docker for ARM compatibility)
- **ORM**: Entity Framework Core 10
- **Migrations**: Applied automatically on startup
- **Seeding**: Idempotent seeder creates permissions, roles, and admin user on first run

### EF Core Migrations

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Portfolio.Infrastructure \
  --startup-project src/Portfolio.Api \
  --output-dir Migrations
```

### Reset Database

```bash
docker compose down -v    # removes volumes (data)
docker compose up -d      # recreates everything fresh
```

## Testing

```bash
# Run all tests (119 tests across 3 projects)
dotnet test Portfolio.sln

# Build only
dotnet build Portfolio.sln
```

See [docs/testing.md](docs/testing.md) for test strategy and infrastructure details.

## Production Deployment

For production, override the development defaults with environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;..."
export ConnectionStrings__Redis="your-redis:6379"
export Jwt__Secret="<production-secret-min-64-chars>"
export Seed__AdminEmail="admin@yourdomain.com"
export Seed__AdminPassword="<strong-password>"
```

The Docker image runs as a non-root user and exposes port 8080 internally:

```bash
docker build -t portfolio-api .
docker run -p 443:8080 portfolio-api
```

See [docs/deployment.md](docs/deployment.md) for the full production guide with Nginx config, security checklist, and scaling notes.

## Contributing

Contributions are welcome. Feel free to open issues or submit pull requests.

## License

MIT
