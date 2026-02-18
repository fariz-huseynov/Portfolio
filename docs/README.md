# Portfolio CMS - Documentation

A .NET 10 portfolio and CMS backend API built with Clean Architecture, featuring AI-powered content generation, JWT authentication with 2FA, hybrid caching, and comprehensive security.

## Table of Contents

| Document | Description |
|----------|-------------|
| [Setup Guide](setup.md) | Get the project running locally or with Docker |
| [Architecture](architecture.md) | Clean Architecture layers, project structure, design patterns |
| [API Reference](api-reference.md) | All endpoints with request/response examples |
| [AI Features](ai-features.md) | AI content generation: providers, configuration, usage |
| [Configuration](configuration.md) | All configuration sections explained |
| [Testing](testing.md) | Test strategy, running tests, test infrastructure |
| [Deployment](deployment.md) | Production deployment guide |

## Quick Links

- **Swagger UI**: `http://localhost:5001/swagger` (development only)
- **Health Check**: `http://localhost:5001/health`
- **Aspire Dashboard**: `http://localhost:18888` (OpenTelemetry traces/logs/metrics)

## Default Credentials (Development)

| Field | Value |
|-------|-------|
| Email | `admin@portfolio.dev` |
| Password | `Admin@123456` |
| Role | SuperAdmin (all permissions) |
