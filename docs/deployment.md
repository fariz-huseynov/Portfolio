# Deployment

## Docker Production Deployment

### 1. Build the Image

```bash
docker build -t portfolio-api .
```

The `Dockerfile` uses a multi-stage build:
- **Build stage**: `mcr.microsoft.com/dotnet/sdk:10.0` — restores, builds, publishes
- **Runtime stage**: `mcr.microsoft.com/dotnet/aspnet:10.0` — minimal runtime image

### 2. Required Environment Variables

Set these when running in production. **Never use the development defaults.**

```bash
# Database
ConnectionStrings__DefaultConnection="Server=your-db;Database=Portfolio;User Id=app_user;Password=<strong>;TrustServerCertificate=False;Encrypt=True"
ConnectionStrings__Redis="your-redis:6379,password=<strong>"

# JWT (CRITICAL — generate a random 64+ character string)
Jwt__Secret="<your-random-64-char-secret>"
Jwt__Issuer="Portfolio"
Jwt__Audience="Portfolio"

# Admin seed (only used on first run when DB is empty)
Seed__AdminEmail="your-admin@yourdomain.com"
Seed__AdminPassword="<strong-password>"

# CORS
Cors__AllowedOrigins__0="https://yourdomain.com"
Cors__AllowedOrigins__1="https://www.yourdomain.com"

# Email (SMTP)
Email__Host="smtp.provider.com"
Email__Port="587"
Email__Username="your-smtp-user"
Email__Password="<smtp-password>"
Email__FromEmail="noreply@yourdomain.com"
Email__FromName="Portfolio"
Email__UseSsl="true"
Email__Enabled="true"

# Frontend URL (used in email links)
Frontend__BaseUrl="https://yourdomain.com"

# AI Providers (optional — only set for providers you use)
AiContent__OpenAi__ApiKey="sk-..."
AiContent__OpenAi__Enabled="true"

# Environment
ASPNETCORE_ENVIRONMENT="Production"
```

### 3. Run with Docker Compose

Create a `docker-compose.production.yml`:

```yaml
services:
  api:
    image: portfolio-api:latest
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=...
      - ConnectionStrings__Redis=...
      - Jwt__Secret=...
      # ... all production env vars
    volumes:
      - uploads:/app/wwwroot/uploads
    restart: unless-stopped

volumes:
  uploads:
```

### 4. Run the Container

```bash
docker compose -f docker-compose.production.yml up -d
```

---

## Database

### Automatic Migrations

Migrations run automatically on startup via `DbSeeder.SeedAsync()`. No manual migration step is needed.

On first startup, the seeder:
1. Applies all pending EF Core migrations
2. Creates the admin user (if no users exist)
3. Seeds all permissions and assigns them to the SuperAdmin role

### Manual Migration (if needed)

```bash
dotnet ef database update \
  --project src/Portfolio.Infrastructure \
  --startup-project src/Portfolio.Api
```

### Backup Recommendations

- Schedule daily SQL Server backups
- Back up the `uploads/` volume (contains user-uploaded files and AI-generated images)
- Redis data is ephemeral (cache only) — no backup needed

---

## Reverse Proxy

In production, run behind a reverse proxy (Nginx, Caddy, or a cloud load balancer) that handles:

- TLS termination (HTTPS)
- HTTP/2
- Request buffering
- Additional rate limiting

### Nginx Example

```nginx
server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    ssl_certificate     /etc/ssl/certs/yourdomain.crt;
    ssl_certificate_key /etc/ssl/private/yourdomain.key;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 120s;
    }

    # SignalR WebSocket support
    location /hubs/ {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_read_timeout 3600s;
    }
}
```

**Important**: The `proxy_read_timeout 120s` on the main location matches the AI generation timeout. For the SignalR hub, set a longer timeout (3600s) to keep WebSocket connections alive.

---

## Observability

### OpenTelemetry

The API exports traces, metrics, and logs via OTLP (OpenTelemetry Protocol). In development, these go to the Aspire Dashboard. In production, point to your collector:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT="https://your-otel-collector:4317"
OTEL_SERVICE_NAME="portfolio-api"
```

Compatible backends: Jaeger, Zipkin, Grafana Tempo, Datadog, New Relic, Azure Monitor.

### Health Checks

```bash
curl https://api.yourdomain.com/health
```

Returns status of SQL Server, Redis, and other dependencies. Use this endpoint for load balancer health checks.

---

## Security Checklist

Before going live:

- [ ] **JWT Secret**: Random 64+ character string (not the dev default)
- [ ] **Admin Password**: Strong password (not `Admin@123456`)
- [ ] **HTTPS Only**: TLS termination via reverse proxy
- [ ] **CORS Origins**: Only your frontend domain(s), not wildcards
- [ ] **Email**: Real SMTP provider configured and enabled
- [ ] **Rate Limits**: Review limits for expected traffic
- [ ] **File Uploads**: Ensure `uploads/` volume has appropriate disk space
- [ ] **Database**: Production-grade SQL Server with backups enabled
- [ ] **Redis**: Password-protected Redis instance
- [ ] **Firewall**: Only expose ports 80/443 publicly; keep 1433/6379 internal
- [ ] **AI Keys**: Only enable providers you actively use; keys are billed per usage
- [ ] **OTEL**: Point to your observability backend for monitoring
- [ ] **Docker**: Run as non-root user (the Dockerfile already handles this)

---

## Scaling Considerations

This is a monolithic application designed for single-instance deployment (typical for personal portfolio sites). If you need to scale:

- **Horizontal**: The stateless API can run multiple instances behind a load balancer. Redis handles distributed cache and output cache. Ensure the `uploads/` volume is shared (NFS, S3, etc.).
- **Database**: Consider Azure SQL or a managed SQL Server for automatic scaling and backups.
- **Redis**: Use a managed Redis service (Azure Cache for Redis, AWS ElastiCache) for high availability.
- **AI Requests**: AI generation is synchronous (120s timeout). For high-volume AI usage, consider implementing a background job queue.
