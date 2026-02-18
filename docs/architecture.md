# Architecture

## Clean Architecture Layers

```
┌──────────────────────────┐
│     Portfolio.Api         │  Controllers, Middleware, Auth, SignalR
├──────────────────────────┤
│  Portfolio.Application    │  DTOs, Service Interfaces & Implementations
├──────────────────────────┤
│ Portfolio.Infrastructure  │  EF Core, Identity, Repositories, AI Providers, Cache
├──────────────────────────┤
│    Portfolio.Domain       │  Entities, Interfaces, Constants
└──────────────────────────┘
     Portfolio.ServiceDefaults → OpenTelemetry, Health Checks
```

**Dependency flow**: Api → Application → Domain ← Infrastructure

- **Domain** has zero dependencies (pure C# entities and interfaces)
- **Application** depends only on Domain
- **Infrastructure** implements Domain interfaces and Application services
- **Api** references all layers and wires them together via DI

## Key Patterns

### Repository Pattern

Generic `IRepository<T>` with specialized repositories for complex queries:

- `IBlogPostRepository` — slug lookup, published filtering
- `IProjectRepository` — slug lookup, published filtering
- `ILeadRepository` — ordered by date, unread filtering
- `IIpRuleRepository` — filtered by type, search
- `IAiGenerationRecordRepository` — paged by user, by operation type

### Service Layer

Business logic lives in `Application/Services/`. Each service:
- Receives dependencies via constructor injection
- Uses `IOptions<T>` for configuration
- Uses `IHybridCacheService` for caching
- Uses `IHtmlSanitizerService` for rich-text sanitization
- Returns DTOs (never entities)

### AI Provider Strategy Pattern

```
IAiProvider (interface)
├── OpenAiProvider    — GPT-4o, DALL-E 3
├── AnthropicProvider — Claude (text only)
├── GeminiProvider    — Gemini, Imagen 3
└── OllamaProvider    — Local models (text only)
```

All providers registered as `IEnumerable<IAiProvider>` in DI. The `AiContentService` orchestrator resolves the correct provider by name.

### Permission-Based RBAC

1. Permissions defined as constants in `Permissions.cs`
2. Auto-seeded to database on startup by `DbSeeder`
3. Assigned to roles (SuperAdmin gets all, Admin gets a subset)
4. Checked via `[HasPermission("Blogs.Edit")]` attribute on endpoints
5. JWT claims carry role info → `PermissionAuthorizationHandler` resolves permissions from roles

### Hybrid Cache (L1 + L2)

```
Request → L1 (IMemoryCache) → L2 (Redis) → Database
```

- L1 miss falls through to L2, L2 miss falls through to the factory function
- Redis failure is swallowed — L1 still works (graceful degradation)
- Cache invalidation removes from both tiers

### IOptions Pattern

All configuration sections use strongly-typed options:

| Class | Section | Location |
|-------|---------|----------|
| `JwtSettings` | `Jwt` | Infrastructure |
| `EmailSettings` | `Email` | Infrastructure |
| `FrontendSettings` | `Frontend` | Infrastructure |
| `CachingOptions` | `Caching` | Application |
| `FileStorageOptions` | `FileStorage` | Application |
| `AiSettings` | `AiContent` | Application |

## Middleware Pipeline

Middleware executes in this order (top to bottom):

1. Serilog request logging
2. IP filter (allow/block rules)
3. Security monitoring (threat detection)
4. Global exception handler
5. Security headers (HSTS, CSP, etc.)
6. Response compression (Brotli + Gzip)
7. HTTPS redirection (production only)
8. Static files
9. CORS
10. Response caching
11. Output cache
12. Authentication
13. Authorization
14. Idempotency (POST requests)
15. Rate limiter
16. Controllers / SignalR
