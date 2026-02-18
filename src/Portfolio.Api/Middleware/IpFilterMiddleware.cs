using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Middleware;

public class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpFilterMiddleware> _logger;
    private readonly TimeSpan _cacheDuration;

    private const string WhitelistCacheKey = "IpFilter:Whitelist";
    private const string BlacklistCacheKey = "IpFilter:Blacklist";

    public IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger, IOptions<CachingOptions> cachingOptions)
    {
        _next = next;
        _logger = logger;
        _cacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.IpRulesMinutes);
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, IMemoryCache cache)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            await _next(context);
            return;
        }

        var (whitelist, blacklist) = await GetCachedRulesAsync(serviceProvider, cache);

        // Whitelist overrides blacklist — check first
        if (whitelist.Contains(ipAddress))
        {
            context.Items["IsWhitelistedIp"] = true;
            await _next(context);
            return;
        }

        if (blacklist.Contains(ipAddress))
        {
            _logger.LogWarning("Blocked request from blacklisted IP {IpAddress} to {RequestPath}",
                ipAddress, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "Access denied." }));
            return;
        }

        await _next(context);
    }

    private async Task<(HashSet<string> Whitelist, HashSet<string> Blacklist)> GetCachedRulesAsync(
        IServiceProvider serviceProvider, IMemoryCache cache)
    {
        var whitelist = await cache.GetOrCreateAsync(WhitelistCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IIpRuleService>();
            return await service.GetActiveWhitelistedIpsAsync();
        }) ?? [];

        var blacklist = await cache.GetOrCreateAsync(BlacklistCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IIpRuleService>();
            return await service.GetActiveBlacklistedIpsAsync();
        }) ?? [];

        return (whitelist, blacklist);
    }
}
