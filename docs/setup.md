# Setup Guide

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (for Docker setup or dependencies)

## Option 1: Docker (Recommended)

Start all services with a single command:

```bash
docker compose up -d
```

This starts 4 services:

| Service | Port | Description |
|---------|------|-------------|
| API | `5001` | Portfolio API |
| SQL Server | `1433` | Azure SQL Edge database |
| Redis | `6379` | Cache (L1+L2 hybrid) |
| Aspire Dashboard | `18888` | OpenTelemetry UI |

The API will be available at `http://localhost:5001/swagger`.

To stop and remove all data:

```bash
docker compose down -v
```

## Option 2: Local Development

### 1. Start Dependencies

```bash
# SQL Server
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Dev@12345" \
  -p 1433:1433 \
  mcr.microsoft.com/azure-sql-edge:latest

# Redis
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

### 2. Run the API

```bash
cd src/Portfolio.Api
dotnet run
```

The API listens on `http://localhost:5001`.

### 3. First Login

1. Open `http://localhost:5001/swagger`
2. Call `POST /api/v1/auth/login` with:
   ```json
   {
     "email": "admin@portfolio.dev",
     "password": "Admin@123456"
   }
   ```
3. Copy the `accessToken` from the response
4. Click "Authorize" in Swagger and enter: `Bearer <your-token>`

## Database Migrations

Migrations run automatically on startup. To create a new migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Portfolio.Infrastructure \
  --startup-project src/Portfolio.Api \
  --output-dir Migrations
```

## Build & Test

```bash
# Build
dotnet build Portfolio.sln

# Run all tests
dotnet test Portfolio.sln
```
