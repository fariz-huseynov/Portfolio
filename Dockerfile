# ── Stage 1: Build ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY Directory.Build.props Directory.Packages.props Portfolio.sln ./
COPY src/Portfolio.Domain/Portfolio.Domain.csproj src/Portfolio.Domain/
COPY src/Portfolio.Application/Portfolio.Application.csproj src/Portfolio.Application/
COPY src/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj src/Portfolio.Infrastructure/
COPY src/Portfolio.ServiceDefaults/Portfolio.ServiceDefaults.csproj src/Portfolio.ServiceDefaults/
COPY src/Portfolio.Api/Portfolio.Api.csproj src/Portfolio.Api/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY src/ src/

# Publish
WORKDIR /src/src/Portfolio.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ─────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser -s /sbin/nologin appuser

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads && chown -R appuser:appuser /app

# Copy published output
COPY --from=build /app/publish .

# Switch to non-root user
USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
