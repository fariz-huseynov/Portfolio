# CLAUDE.md

Guidance for Claude Code when working with this repository.

## Project Overview

Personal portfolio / CMS backend API. Monolithic .NET 10 application with Clean Architecture.

- **Backend**: ASP.NET Core 10 with Clean Architecture
- **Database**: SQL Server (Azure SQL Edge in Docker)
- **Cache**: Redis (L1 in-memory + L2 distributed)
- **Observability**: OpenTelemetry → Aspire Dashboard
- **Auth**: JWT with refresh tokens, permission-based RBAC, TOTP 2FA

## Quick Start

### Docker (recommended)

```bash
docker compose up -d
```

Services: API (5001), SQL Server (1433), Redis (6379), Aspire Dashboard (18888).

### Local Development

```bash
# Start dependencies
docker run -d --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Dev@12345" -p 1433:1433 mcr.microsoft.com/azure-sql-edge:latest
docker run -d --name redis -p 6379:6379 redis:7-alpine

# Run the API
cd src/Portfolio.Api
dotnet run
# Listens on http://localhost:5001
```

**Login**: admin@portfolio.dev / Admin@123456 (SuperAdmin with all 39 permissions)

**Swagger**: http://localhost:5001/swagger

## Architecture

```
Portfolio.Domain          → Entities, interfaces, constants
Portfolio.Application     → DTOs, service interfaces, business logic
Portfolio.Infrastructure  → EF Core, Identity, repositories, Redis cache, email
Portfolio.Api             → Controllers, middleware, auth, SignalR hubs
Portfolio.ServiceDefaults → OpenTelemetry, health checks, service discovery
```

## Key Paths

| Path | Description |
|------|-------------|
| `src/Portfolio.Api/Program.cs` | App startup, middleware pipeline |
| `src/Portfolio.Api/Controllers/` | REST API controllers (15 files) |
| `src/Portfolio.Api/Middleware/` | IP filter, security headers, exception handling, idempotency |
| `src/Portfolio.Api/Authorization/` | Permission-based auth handler |
| `src/Portfolio.Application/Interfaces/` | Service contracts |
| `src/Portfolio.Application/DTOs/` | Request/response DTOs |
| `src/Portfolio.Application/Services/` | Business logic implementations |
| `src/Portfolio.Infrastructure/Data/` | DbContext, repositories, seeder |
| `src/Portfolio.Infrastructure/Identity/` | Auth, users, roles, JWT, CAPTCHA |
| `src/Portfolio.Infrastructure/Caching/` | L1/L2 hybrid cache service |
| `src/Portfolio.Infrastructure/DependencyInjection.cs` | All service registrations |
| `src/Portfolio.Domain/Entities/` | Domain entities |
| `src/Portfolio.Domain/Constants/Permissions.cs` | All 39 permission constants |

## Commands

```bash
# Build
dotnet build Portfolio.sln

# Run (local dev)
cd src/Portfolio.Api && dotnet run

# Docker
docker compose up -d
docker compose down -v   # reset everything

# EF Core migration
dotnet ef migrations add <Name> \
  --project src/Portfolio.Infrastructure \
  --startup-project src/Portfolio.Api \
  --output-dir Migrations
```

## Configuration

All dev config lives in `src/Portfolio.Api/appsettings.Development.json`. No user-secrets needed.

| Key | Value (dev) |
|-----|-------------|
| ConnectionStrings:DefaultConnection | Server=localhost;Database=Portfolio_Dev;User Id=sa;Password=Dev@12345;... |
| ConnectionStrings:Redis | localhost:6379 |
| Jwt:Secret | ThisIsADevelopmentSecretKeyForJwtTokenGeneration123! |
| Seed:AdminEmail | admin@portfolio.dev |
| Seed:AdminPassword | Admin@123456 |

## Patterns

### Adding a Permission

1. Add constant in `Portfolio.Domain/Constants/Permissions.cs`
2. Add to `Permissions.All` array
3. Restart — auto-seeded and assigned to SuperAdmin

### Adding an API Endpoint

1. Create DTOs in `Application/DTOs/`
2. Create service interface in `Application/Interfaces/`
3. Implement service in `Infrastructure/Services/`
4. Register in `Infrastructure/DependencyInjection.cs`
5. Create controller in `Api/Controllers/` with `[HasPermission]` attributes

### Caching

- `IHybridCacheService` — L1 (memory) + L2 (Redis) hybrid cache
- Output cache policy `"PublicContent"` — 5-minute Redis-backed HTTP cache for public GETs
- Cache eviction: call `RemoveByPrefixAsync` or tag-based eviction on content mutations

### Authentication Flow

1. `POST /api/v1/auth/login` → JWT access token + refresh token
2. `Authorization: Bearer <token>` on every request
3. Backend validates JWT, extracts permission claims
4. `[HasPermission("Blogs.Edit")]` checks claims against requirement
5. On 401 → client calls `/api/v1/auth/refresh` → retries

## Middleware Pipeline Order

```
Serilog request logging
IP filter
Security monitoring
Global exception handler
Security headers
Response compression
HTTPS redirection (production)
Static files
CORS
Response caching
Output cache
Authentication
Authorization
Idempotency
Rate limiter
Controllers / SignalR
Health checks
```

## Docker

`docker-compose.yml` runs 4 services with hardcoded dev credentials (designed for public repo).

- API port: 5001 → 8080 (internal)
- OTEL endpoint: `http://aspire-dashboard:18889`
- All env vars override `appsettings.json` via `__` notation (e.g., `Jwt__Secret`)

## Security Features

- Rate limiting (6 policies: Auth, ForgotPassword, TwoFactorVerify, LeadSubmit, PublicApi, Global)
- Security headers (HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy)
- HTML sanitization on all rich-text content
- CAPTCHA on lead submission (Lazy.Captcha.Core)
- File upload validation (extension, MIME, magic bytes, size limit 10MB)
- IP allow/block rules
- API idempotency via `Idempotency-Key` header (POST requests, Redis-backed, 24h TTL)
- Self-action prevention (can't delete/disable own account)
