using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private const string CacheKeyPrefix = "permissions:";

    public PermissionService(AppDbContext dbContext, IMemoryCache cache, IOptions<CachingOptions> cachingOptions)
    {
        _dbContext = dbContext;
        _cache = cache;
        _cacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.PermissionsMinutes);
    }

    public async Task<IReadOnlySet<string>> GetPermissionsForRolesAsync(
        IEnumerable<string> roleNames, CancellationToken ct = default)
    {
        var sortedKey = CacheKeyPrefix + string.Join(",", roleNames.OrderBy(r => r));

        if (_cache.TryGetValue(sortedKey, out IReadOnlySet<string>? cached) && cached is not null)
            return cached;

        var roleIds = await _dbContext.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var permissionNames = await _dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);

        var result = new HashSet<string>(permissionNames);

        _cache.Set(sortedKey, (IReadOnlySet<string>)result, _cacheDuration);
        return result;
    }

    public void InvalidateCache()
    {
        // IMemoryCache doesn't have a clear-all, but we can use a cache token approach.
        // For simplicity, we rely on the 5-minute TTL and accept eventual consistency.
        // In a production system, consider using a CancellationTokenSource for cache eviction.
        if (_cache is MemoryCache memoryCache)
            memoryCache.Compact(1.0);
    }
}
