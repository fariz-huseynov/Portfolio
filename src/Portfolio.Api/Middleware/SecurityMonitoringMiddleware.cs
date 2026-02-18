using Microsoft.Extensions.Caching.Memory;

namespace Portfolio.Api.Middleware;

public class SecurityMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityMonitoringMiddleware> _logger;

    private static readonly string[] SuspiciousPaths =
    [
        "/wp-admin", "/wp-login", "/wp-content", "/wp-includes",
        "/phpmyadmin", "/pma", "/myadmin",
        "/.env", "/.git", "/.htaccess", "/.aws",
        "/admin/config", "/elmah.axd", "/trace.axd",
        "/server-status", "/server-info",
        "/cgi-bin", "/scripts", "/xmlrpc.php"
    ];

    private static readonly string[] SqlInjectionPatterns =
    [
        "' OR ", "' or ", "1=1", "UNION SELECT", "union select",
        "DROP TABLE", "drop table", "'; --", "' --",
        "EXEC(", "exec(", "xp_cmdshell"
    ];

    public SecurityMonitoringMiddleware(RequestDelegate next, ILogger<SecurityMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var requestPath = context.Request.Path.Value ?? "/";

        // Pre-request checks
        if (IsSuspiciousPath(requestPath))
        {
            LogSecurityEvent(cache, ipAddress, "SuspiciousPath", requestPath);
        }

        if (ContainsPathTraversal(requestPath))
        {
            LogSecurityEvent(cache, ipAddress, "PathTraversal", requestPath);
        }

        var queryString = context.Request.QueryString.Value;
        if (!string.IsNullOrEmpty(queryString) && ContainsSqlInjection(queryString))
        {
            LogSecurityEvent(cache, ipAddress, "SqlInjectionAttempt", requestPath);
        }

        await _next(context);

        // Post-request checks based on response status
        switch (context.Response.StatusCode)
        {
            case 429:
                LogSecurityEvent(cache, ipAddress, "RateLimitViolation", requestPath);
                break;
            case 401:
                LogSecurityEvent(cache, ipAddress, "UnauthorizedAccess", requestPath);
                break;
            case 403 when context.Items.ContainsKey("IsWhitelistedIp") == false:
                LogSecurityEvent(cache, ipAddress, "ForbiddenAccess", requestPath);
                break;
        }
    }

    private void LogSecurityEvent(IMemoryCache cache, string ipAddress, string threatType, string requestPath)
    {
        var cacheKey = $"SecurityViolation:{ipAddress}";
        var count = cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return 0;
        });

        count++;
        cache.Set(cacheKey, count, TimeSpan.FromMinutes(15));

        _logger.LogWarning(
            "SecurityEvent: {ThreatType} detected from IP {IpAddress} on {RequestPath} (ViolationCount: {ViolationCount})",
            threatType, ipAddress, requestPath, count);
    }

    private static bool IsSuspiciousPath(string path)
    {
        var lowerPath = path.ToLowerInvariant();
        return Array.Exists(SuspiciousPaths, p => lowerPath.StartsWith(p, StringComparison.Ordinal));
    }

    private static bool ContainsPathTraversal(string path)
        => path.Contains("../") || path.Contains("..\\");

    private static bool ContainsSqlInjection(string input)
        => Array.Exists(SqlInjectionPatterns, pattern => input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
}
