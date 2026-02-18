using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Portfolio.Infrastructure.Caching;
using Xunit;

namespace Portfolio.Infrastructure.Tests.Caching;

public class HybridCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly HybridCacheService _sut;

    public HybridCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _distributedCache = Substitute.For<IDistributedCache>();
        _sut = new HybridCacheService(_memoryCache, _distributedCache);
    }

    #region Test helpers

    private sealed record TestItem(string Name, int Value);

    #endregion

    #region GetOrCreateAsync

    [Fact]
    public async Task GetOrCreateAsync_ReturnsFromMemoryCache_WhenItemExistsInMemory()
    {
        // Arrange
        const string key = "test-key";
        var expected = new TestItem("cached", 42);
        _memoryCache.Set(key, expected);

        var factoryCalled = false;

        // Act
        var result = await _sut.GetOrCreateAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult(new TestItem("fresh", 99));
        });

        // Assert
        result.Should().BeSameAs(expected);
        factoryCalled.Should().BeFalse("factory should not be called when item is in memory cache");
        await _distributedCache.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_CallsFactory_WhenNothingCached()
    {
        // Arrange
        const string key = "missing-key";
        var expected = new TestItem("fresh", 99);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result = await _sut.GetOrCreateAsync(key, () => Task.FromResult(expected));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetOrCreateAsync_PopulatesMemoryCache_AfterFactoryCall()
    {
        // Arrange
        const string key = "populate-key";
        var expected = new TestItem("fresh", 99);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        await _sut.GetOrCreateAsync(key, () => Task.FromResult(expected));

        // Assert
        _memoryCache.TryGetValue(key, out TestItem? cached).Should().BeTrue();
        cached.Should().Be(expected);
    }

    [Fact]
    public async Task GetOrCreateAsync_PopulatesMemoryCache_FromDistributedCache()
    {
        // Arrange
        const string key = "redis-key";
        var expected = new TestItem("from-redis", 7);
        var serialized = JsonSerializer.SerializeToUtf8Bytes(expected);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns(serialized);

        var factoryCalled = false;

        // Act
        var result = await _sut.GetOrCreateAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult(new TestItem("should-not-be-used", 0));
        });

        // Assert
        result.Should().Be(expected);
        factoryCalled.Should().BeFalse("factory should not be called when item is in distributed cache");

        _memoryCache.TryGetValue(key, out TestItem? cached).Should().BeTrue();
        cached.Should().Be(expected);
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsNull_WhenFactoryReturnsNull()
    {
        // Arrange
        const string key = "null-factory-key";
        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result = await _sut.GetOrCreateAsync<TestItem>(key, () => Task.FromResult<TestItem>(null!));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_FallsBackToFactory_WhenDistributedCacheThrows()
    {
        // Arrange
        const string key = "redis-fail-key";
        var expected = new TestItem("fallback", 1);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis unavailable"));

        // Act
        var result = await _sut.GetOrCreateAsync(key, () => Task.FromResult(expected));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetOrCreateAsync_StillPopulatesMemoryCache_WhenDistributedCacheSetThrows()
    {
        // Arrange
        const string key = "redis-set-fail-key";
        var expected = new TestItem("value", 5);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        _distributedCache.SetAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        ).ThrowsAsync(new InvalidOperationException("Redis unavailable"));

        // Act
        var result = await _sut.GetOrCreateAsync(key, () => Task.FromResult(expected));

        // Assert
        result.Should().Be(expected);
        _memoryCache.TryGetValue(key, out TestItem? cached).Should().BeTrue();
        cached.Should().Be(expected);
    }

    [Fact]
    public async Task GetOrCreateAsync_WritesToDistributedCache_AfterFactoryCall()
    {
        // Arrange
        const string key = "write-redis-key";
        var expected = new TestItem("persisted", 10);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        await _sut.GetOrCreateAsync(key, () => Task.FromResult(expected));

        // Assert
        await _distributedCache.Received(1).SetAsync(
            key,
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region RemoveAsync

    [Fact]
    public async Task RemoveAsync_RemovesFromBothCaches()
    {
        // Arrange
        const string key = "remove-key";
        _memoryCache.Set(key, new TestItem("to-remove", 1));

        // Act
        await _sut.RemoveAsync(key);

        // Assert
        _memoryCache.TryGetValue(key, out _).Should().BeFalse("item should be removed from memory cache");
        await _distributedCache.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow_WhenDistributedCacheFails()
    {
        // Arrange
        const string key = "remove-fail-key";
        _memoryCache.Set(key, new TestItem("value", 1));

        _distributedCache.RemoveAsync(key, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis unavailable"));

        // Act
        var act = () => _sut.RemoveAsync(key);

        // Assert
        await act.Should().NotThrowAsync();
        _memoryCache.TryGetValue(key, out _).Should().BeFalse("memory cache should still be cleared");
    }

    #endregion
}
