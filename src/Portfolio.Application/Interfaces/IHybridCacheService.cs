namespace Portfolio.Application.Interfaces;

public interface IHybridCacheService
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? memoryCacheDuration = null,
        TimeSpan? distributedCacheDuration = null,
        CancellationToken ct = default) where T : class;

    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByTagAsync(string tag, CancellationToken ct = default);
}
