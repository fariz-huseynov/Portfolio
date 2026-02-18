using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.Caching;

public class HybridCacheService : IHybridCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly Dictionary<string, HashSet<string>> _tagIndex = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly TimeSpan DefaultMemoryDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultDistributedDuration = TimeSpan.FromMinutes(10);

    public HybridCacheService(IMemoryCache memoryCache, IDistributedCache distributedCache)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? memoryCacheDuration = null,
        TimeSpan? distributedCacheDuration = null,
        CancellationToken ct = default) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cached))
            return cached;

        try
        {
            var redisBytes = await _distributedCache.GetAsync(key, ct);
            if (redisBytes is not null)
            {
                var fromRedis = JsonSerializer.Deserialize<T>(redisBytes);
                if (fromRedis is not null)
                {
                    _memoryCache.Set(key, fromRedis, memoryCacheDuration ?? DefaultMemoryDuration);
                    return fromRedis;
                }
            }
        }
        catch
        {
            // Redis unavailable — fall through to factory
        }

        var result = await factory();
        if (result is null)
            return null;

        _memoryCache.Set(key, result, memoryCacheDuration ?? DefaultMemoryDuration);

        try
        {
            var serialized = JsonSerializer.SerializeToUtf8Bytes(result);
            await _distributedCache.SetAsync(key, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = distributedCacheDuration ?? DefaultDistributedDuration
            }, ct);
        }
        catch
        {
            // Redis unavailable — memory cache still works
        }

        return result;
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _memoryCache.Remove(key);
        try { await _distributedCache.RemoveAsync(key, ct); } catch { }
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_tagIndex.TryGetValue(tag, out var keys))
            {
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                    try { await _distributedCache.RemoveAsync(key, ct); } catch { }
                }
                _tagIndex.Remove(tag);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
