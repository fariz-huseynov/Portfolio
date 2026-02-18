#pragma warning disable CS8620 // Nullability mismatch in NSubstitute generic type inference

using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;
using Xunit;

namespace Portfolio.Application.Tests.Services;

public class BlogPostServiceTests
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IHybridCacheService _cache;
    private readonly BlogPostService _sut;

    public BlogPostServiceTests()
    {
        _blogPostRepository = Substitute.For<IBlogPostRepository>();
        _htmlSanitizer = Substitute.For<IHtmlSanitizerService>();
        _cache = Substitute.For<IHybridCacheService>();

        var cachingOptions = Options.Create(new CachingOptions
        {
            BlogPostMinutes = 30,
            PublishedListMinutes = 15
        });

        // Default: sanitizer returns input unchanged
        _htmlSanitizer.Sanitize(Arg.Any<string>()).Returns(callInfo => callInfo.ArgAt<string>(0));

        _sut = new BlogPostService(_blogPostRepository, _htmlSanitizer, _cache, cachingOptions);
    }

    #region GetPublishedPostsAsync

    [Fact]
    public async Task GetPublishedPostsAsync_ReturnsCachedData_WhenCacheHit()
    {
        // Arrange
        var cachedPosts = new List<BlogPostDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Cached Post", Slug = "cached-post" }
        };

        _cache.GetOrCreateAsync(
            Arg.Is<string>(k => k == CacheKeys.BlogPostsPublished),
            Arg.Any<Func<Task<IReadOnlyList<BlogPostDto>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<IReadOnlyList<BlogPostDto>?>(cachedPosts));

        // Act
        var result = await _sut.GetPublishedPostsAsync();

        // Assert
        result.Should().BeEquivalentTo(cachedPosts);
        await _blogPostRepository.DidNotReceive().GetPublishedAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPublishedPostsAsync_CallsFactory_WhenCacheMiss()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new() { Id = Guid.NewGuid(), Title = "Post 1", Slug = "post-1", Content = "Content", CreatedAt = DateTime.UtcNow }
        };

        _blogPostRepository.GetPublishedAsync(Arg.Any<CancellationToken>()).Returns(posts);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IReadOnlyList<BlogPostDto>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<IReadOnlyList<BlogPostDto>>>>(1).Invoke());

        // Act
        var result = await _sut.GetPublishedPostsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Post 1");
        await _blogPostRepository.Received(1).GetPublishedAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetPostBySlugAsync

    [Fact]
    public async Task GetPostBySlugAsync_ReturnsPost_WhenSlugExists()
    {
        // Arrange
        var slug = "test-post";
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Slug = slug,
            Summary = "Summary",
            Content = "Content",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _blogPostRepository.GetBySlugAsync(slug, Arg.Any<CancellationToken>()).Returns(post);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<BlogPostDto>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<BlogPostDto>>>(1).Invoke());

        // Act
        var result = await _sut.GetPostBySlugAsync(slug);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Post");
        result.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ReturnsNull_WhenSlugDoesNotExist()
    {
        // Arrange
        _blogPostRepository.GetBySlugAsync("non-existent", Arg.Any<CancellationToken>())
            .Returns((BlogPost?)null);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<BlogPostDto>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<BlogPostDto>>>(1).Invoke());

        // Act
        // The service maps null to null! which becomes default/null for reference type
        var result = await _sut.GetPostBySlugAsync("non-existent");

        // Assert — the factory returns null! for missing slug, so cached result is null
        // The behavior depends on how the cache handles null! — it may return null
        result.Should().BeNull();
    }

    #endregion

    #region CreatePostAsync

    [Fact]
    public async Task CreatePostAsync_SanitizesContent()
    {
        // Arrange
        var dto = new BlogPostCreateDto
        {
            Title = "New Post",
            Slug = "new-post",
            Summary = "Summary",
            Content = "<script>alert('xss')</script><p>Hello</p>",
            IsPublished = false
        };

        _htmlSanitizer.Sanitize(dto.Content).Returns("<p>Hello</p>");

        // Act
        var result = await _sut.CreatePostAsync(dto);

        // Assert
        _htmlSanitizer.Received(1).Sanitize(dto.Content);
        result.Content.Should().Be("<p>Hello</p>");
    }

    [Fact]
    public async Task CreatePostAsync_WhenPublished_SetsPublishedAt()
    {
        // Arrange
        var dto = new BlogPostCreateDto
        {
            Title = "Published Post",
            Slug = "published-post",
            Summary = "Summary",
            Content = "<p>Content</p>",
            IsPublished = true
        };

        // Act
        var result = await _sut.CreatePostAsync(dto);

        // Assert
        result.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        result.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreatePostAsync_WhenNotPublished_DoesNotSetPublishedAt()
    {
        // Arrange
        var dto = new BlogPostCreateDto
        {
            Title = "Draft Post",
            Slug = "draft-post",
            Summary = "Summary",
            Content = "<p>Content</p>",
            IsPublished = false
        };

        // Act
        var result = await _sut.CreatePostAsync(dto);

        // Assert
        result.IsPublished.Should().BeFalse();
        result.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreatePostAsync_InvalidatesCache()
    {
        // Arrange
        var dto = new BlogPostCreateDto
        {
            Title = "Post",
            Slug = "post",
            Summary = "Summary",
            Content = "Content",
            IsPublished = false
        };

        // Act
        await _sut.CreatePostAsync(dto);

        // Assert
        await _cache.Received(1).RemoveAsync(
            Arg.Is<string>(k => k == CacheKeys.BlogPostsPublished),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePostAsync_CallsRepositoryAdd()
    {
        // Arrange
        var dto = new BlogPostCreateDto
        {
            Title = "Post",
            Slug = "post",
            Summary = "Summary",
            Content = "Content",
            IsPublished = false
        };

        // Act
        await _sut.CreatePostAsync(dto);

        // Assert
        await _blogPostRepository.Received(1).AddAsync(
            Arg.Is<BlogPost>(p => p.Title == "Post" && p.Slug == "post"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdatePostAsync

    [Fact]
    public async Task UpdatePostAsync_ThrowsKeyNotFoundException_WhenPostDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((BlogPost?)null);

        var dto = new BlogPostUpdateDto
        {
            Title = "Updated",
            Slug = "updated",
            Summary = "Summary",
            Content = "Content"
        };

        // Act
        var act = () => _sut.UpdatePostAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task UpdatePostAsync_SanitizesContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPost = new BlogPost
        {
            Id = id,
            Title = "Old",
            Slug = "old",
            Summary = "Old summary",
            Content = "Old content",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingPost);

        var dto = new BlogPostUpdateDto
        {
            Title = "Updated",
            Slug = "updated",
            Summary = "New summary",
            Content = "<script>bad</script><p>Good</p>",
            IsPublished = false
        };

        _htmlSanitizer.Sanitize(dto.Content).Returns("<p>Good</p>");

        // Act
        var result = await _sut.UpdatePostAsync(id, dto);

        // Assert
        _htmlSanitizer.Received(1).Sanitize(dto.Content);
        result.Content.Should().Be("<p>Good</p>");
    }

    [Fact]
    public async Task UpdatePostAsync_SetsPublishedAt_OnPublishTransition()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPost = new BlogPost
        {
            Id = id,
            Title = "Draft",
            Slug = "draft",
            Summary = "Summary",
            Content = "Content",
            IsPublished = false,
            PublishedAt = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingPost);

        var dto = new BlogPostUpdateDto
        {
            Title = "Now Published",
            Slug = "now-published",
            Summary = "Summary",
            Content = "Content",
            IsPublished = true
        };

        // Act
        var result = await _sut.UpdatePostAsync(id, dto);

        // Assert
        result.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        result.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdatePostAsync_DoesNotChangePublishedAt_WhenAlreadyPublished()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalPublishedAt = DateTime.UtcNow.AddDays(-5);
        var existingPost = new BlogPost
        {
            Id = id,
            Title = "Published",
            Slug = "published",
            Summary = "Summary",
            Content = "Content",
            IsPublished = true,
            PublishedAt = originalPublishedAt,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingPost);

        var dto = new BlogPostUpdateDto
        {
            Title = "Updated Published",
            Slug = "updated-published",
            Summary = "New Summary",
            Content = "New Content",
            IsPublished = true
        };

        // Act
        var result = await _sut.UpdatePostAsync(id, dto);

        // Assert
        result.PublishedAt.Should().Be(originalPublishedAt);
    }

    [Fact]
    public async Task UpdatePostAsync_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPost = new BlogPost
        {
            Id = id,
            Title = "Post",
            Slug = "post",
            Summary = "Summary",
            Content = "Content",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingPost);

        var dto = new BlogPostUpdateDto
        {
            Title = "Updated",
            Slug = "updated-slug",
            Summary = "Summary",
            Content = "Content",
            IsPublished = false
        };

        // Act
        await _sut.UpdatePostAsync(id, dto);

        // Assert
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostsPublished, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostBySlug("updated-slug"), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeletePostAsync

    [Fact]
    public async Task DeletePostAsync_ThrowsKeyNotFoundException_WhenPostDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((BlogPost?)null);

        // Act
        var act = () => _sut.DeletePostAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task DeletePostAsync_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPost = new BlogPost
        {
            Id = id,
            Title = "To Delete",
            Slug = "to-delete",
            Content = "Content",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingPost);

        // Act
        await _sut.DeletePostAsync(id);

        // Assert
        await _blogPostRepository.Received(1).DeleteAsync(existingPost, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostsPublished, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostBySlug("to-delete"), Arg.Any<CancellationToken>());
    }

    #endregion

    #region PublishPostAsync

    [Fact]
    public async Task PublishPostAsync_SetsPublishedAtAndSaves()
    {
        // Arrange
        var id = Guid.NewGuid();
        var post = new BlogPost
        {
            Id = id,
            Title = "Draft Post",
            Slug = "draft-post",
            Content = "Content",
            IsPublished = false,
            PublishedAt = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        await _sut.PublishPostAsync(id);

        // Assert
        post.IsPublished.Should().BeTrue();
        post.PublishedAt.Should().NotBeNull();
        post.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        post.UpdatedAt.Should().NotBeNull();
        await _blogPostRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostsPublished, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPostAsync_ThrowsKeyNotFoundException_WhenPostDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((BlogPost?)null);

        // Act
        var act = () => _sut.PublishPostAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task PublishPostAsync_DoesNothing_WhenAlreadyPublished()
    {
        // Arrange
        var id = Guid.NewGuid();
        var post = new BlogPost
        {
            Id = id,
            Title = "Published Post",
            Slug = "published-post",
            Content = "Content",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        await _sut.PublishPostAsync(id);

        // Assert
        await _blogPostRepository.DidNotReceive().UpdateAsync(Arg.Any<BlogPost>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UnpublishPostAsync

    [Fact]
    public async Task UnpublishPostAsync_ClearsIsPublishedAndSaves()
    {
        // Arrange
        var id = Guid.NewGuid();
        var post = new BlogPost
        {
            Id = id,
            Title = "Published Post",
            Slug = "published-post",
            Content = "Content",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        await _sut.UnpublishPostAsync(id);

        // Assert
        post.IsPublished.Should().BeFalse();
        post.UpdatedAt.Should().NotBeNull();
        await _blogPostRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostsPublished, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.BlogPostBySlug("published-post"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnpublishPostAsync_ThrowsKeyNotFoundException_WhenPostDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((BlogPost?)null);

        // Act
        var act = () => _sut.UnpublishPostAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UnpublishPostAsync_DoesNothing_WhenAlreadyUnpublished()
    {
        // Arrange
        var id = Guid.NewGuid();
        var post = new BlogPost
        {
            Id = id,
            Title = "Draft Post",
            Slug = "draft-post",
            Content = "Content",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _blogPostRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        await _sut.UnpublishPostAsync(id);

        // Assert
        await _blogPostRepository.DidNotReceive().UpdateAsync(Arg.Any<BlogPost>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
