using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.DTOs.Auth;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Api.Tests;

/// <summary>
/// A migrator that calls EnsureCreated instead of applying SQL Server migrations.
/// This allows DbSeeder.SeedAsync to succeed without actual migration files.
/// Uses ICurrentDbContext from EF Core's internal services (not the app DI container).
/// </summary>
internal sealed class EnsureCreatedMigrator : IMigrator
{
    private readonly ICurrentDbContext _currentDbContext;

    public EnsureCreatedMigrator(ICurrentDbContext currentDbContext)
    {
        _currentDbContext = currentDbContext;
    }

    public string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        => string.Empty;

    public void Migrate(string? targetMigration = null)
        => _currentDbContext.Context.Database.EnsureCreated();

    public async Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
        => await _currentDbContext.Context.Database.EnsureCreatedAsync(cancellationToken);

    public bool HasPendingModelChanges() => false;
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Keep the SQLite connection open for the lifetime of the factory.
    // SQLite in-memory databases are destroyed when the connection closes.
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Override configuration values before services are built
        builder.UseSetting("Jwt:Secret", "ThisIsATestJwtSecretKeyThatIsAtLeast32Characters!");
        builder.UseSetting("Jwt:Issuer", "Portfolio");
        builder.UseSetting("Jwt:Audience", "Portfolio");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "60");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");
        builder.UseSetting("Seed:AdminEmail", "admin@portfolio.dev");
        builder.UseSetting("Seed:AdminPassword", "Admin@123456");
        builder.UseSetting("Email:Enabled", "false");
        builder.UseSetting("Frontend:BaseUrl", "https://localhost:3000");
        builder.UseSetting("Frontend:ResetPasswordPath", "/reset-password");
        builder.UseSetting("ConnectionStrings:Redis", "");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "");

        // ConfigureTestServices runs AFTER the application's ConfigureServices,
        // so all app-registered services (including SQL Server DbContext) are present
        // and can be properly replaced.
        builder.ConfigureTestServices(services =>
        {
            // ── Replace SQL Server DbContext with SQLite in-memory ────────
            // Remove ALL descriptors related to AppDbContext and its options,
            // including the IDbContextOptionsConfiguration that wires up UseSqlServer.
            var descriptorsToRemove = services
                .Where(d =>
                {
                    var svcName = d.ServiceType?.FullName ?? "";
                    var implName = d.ImplementationType?.FullName ?? "";

                    return d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                           d.ServiceType == typeof(DbContextOptions) ||
                           d.ServiceType == typeof(AppDbContext) ||
                           // Remove the IDbContextOptionsConfiguration<AppDbContext> that
                           // contains the UseSqlServer call
                           svcName.Contains("IDbContextOptionsConfiguration") ||
                           // Remove any SQL Server-specific services
                           svcName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                           implName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
                // Replace the internal EF Core migrator with our no-op version
                // that calls EnsureCreated instead of applying SQL Server migrations.
                options.ReplaceService<IMigrator, EnsureCreatedMigrator>();
            });

            // ── Replace Redis distributed cache with in-memory ───────────
            var cacheDescriptors = services
                .Where(d =>
                {
                    var svcName = d.ServiceType?.FullName ?? "";
                    var implName = d.ImplementationType?.FullName ?? "";

                    return d.ServiceType == typeof(IDistributedCache) ||
                           implName.Contains("Redis", StringComparison.OrdinalIgnoreCase) ||
                           implName.Contains("StackExchange", StringComparison.OrdinalIgnoreCase) ||
                           svcName.Contains("Redis", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            foreach (var descriptor in cacheDescriptors)
                services.Remove(descriptor);

            services.AddDistributedMemoryCache();

            // ── Replace Redis output cache store with in-memory ──────────
            var outputCacheDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(IOutputCacheStore))
                .ToList();
            foreach (var descriptor in outputCacheDescriptors)
                services.Remove(descriptor);

            // Re-add the default in-memory output cache (registers IOutputCacheStore)
            services.AddOutputCache();

            // ── Remove the database health check (it needs a real SQL Server) ──
            // The "self" check from AddServiceDefaults is fine; just remove the
            // EF Core "database" check registered by Infrastructure.
            var healthCheckRegistrations = services
                .Where(d =>
                {
                    var svcName = d.ServiceType?.FullName ?? "";
                    var implName = d.ImplementationType?.FullName ?? "";
                    return (svcName.Contains("IHealthCheck") && implName.Contains("DbContext")) ||
                           implName.Contains("DbContextHealthCheck", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            foreach (var descriptor in healthCheckRegistrations)
                services.Remove(descriptor);

            // ── Configure JWT settings for testing ───────────────────────
            services.Configure<Portfolio.Infrastructure.Identity.JwtSettings>(options =>
            {
                options.Secret = "ThisIsATestJwtSecretKeyThatIsAtLeast32Characters!";
                options.Issuer = "Portfolio";
                options.Audience = "Portfolio";
                options.AccessTokenExpirationMinutes = 60;
                options.RefreshTokenExpirationDays = 7;
            });
        });
    }

    /// <summary>
    /// Logs in as the seeded admin user and returns a valid JWT access token.
    /// </summary>
    public async Task<string> GetAuthTokenAsync()
    {
        var client = CreateClient();

        var loginDto = new LoginDto
        {
            Email = "admin@portfolio.dev",
            Password = "Admin@123456"
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse?.AccessToken
            ?? throw new InvalidOperationException("Failed to obtain auth token from login response.");
    }

    /// <summary>
    /// Creates an HttpClient with a valid Authorization header for the seeded admin user.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var token = await GetAuthTokenAsync();
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
