using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Portfolio.Api.Middleware;

public class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
    {
        if (context.Request.Method != HttpMethods.Post ||
            !context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next(context);
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";

        try
        {
            var cachedResponse = await cache.GetAsync(cacheKey);
            if (cachedResponse is not null)
            {
                var cached = JsonSerializer.Deserialize<CachedResponse>(cachedResponse);
                if (cached is not null)
                {
                    context.Response.StatusCode = cached.StatusCode;
                    context.Response.ContentType = cached.ContentType;
                    await context.Response.Body.WriteAsync(cached.Body);
                    return;
                }
            }
        }
        catch
        {
            // Redis unavailable — proceed without idempotency
        }

        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = memoryStream.ToArray();

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            try
            {
                var toCache = new CachedResponse
                {
                    StatusCode = context.Response.StatusCode,
                    ContentType = context.Response.ContentType ?? "application/json",
                    Body = responseBody
                };

                await cache.SetAsync(cacheKey,
                    JsonSerializer.SerializeToUtf8Bytes(toCache),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                    });
            }
            catch
            {
                // Redis unavailable — response still goes through
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private sealed class CachedResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public byte[] Body { get; set; } = [];
    }
}
