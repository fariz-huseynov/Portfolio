using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class BlogPostService : IBlogPostService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IHybridCacheService _cache;
    private readonly TimeSpan _slugCacheDuration;
    private readonly TimeSpan _listCacheDuration;

    public BlogPostService(IBlogPostRepository blogPostRepository, IHtmlSanitizerService htmlSanitizer, IHybridCacheService cache, IOptions<CachingOptions> cachingOptions)
    {
        _blogPostRepository = blogPostRepository;
        _htmlSanitizer = htmlSanitizer;
        _cache = cache;
        _slugCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.BlogPostMinutes);
        _listCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.PublishedListMinutes);
    }

    public async Task<IReadOnlyList<BlogPostDto>> GetPublishedPostsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.BlogPostsPublished,
            async () =>
            {
                var posts = await _blogPostRepository.GetPublishedAsync(ct);
                return (IReadOnlyList<BlogPostDto>)posts.Select(MapToDto).ToList();
            },
            _listCacheDuration,
            null,
            ct
        ) ?? new List<BlogPostDto>();
    }

    public async Task<PagedResult<BlogPostDto>> GetPublishedPostsPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _blogPostRepository.GetPublishedPagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<BlogPostDto>.Create(items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<BlogPostDto?> GetPostBySlugAsync(string slug, CancellationToken ct = default)
    {
        var cached = await _cache.GetOrCreateAsync(
            CacheKeys.BlogPostBySlug(slug),
            async () =>
            {
                var post = await _blogPostRepository.GetBySlugAsync(slug, ct);
                return post is not null ? MapToDto(post) : null!;
            },
            _slugCacheDuration,
            null,
            ct
        );
        return cached;
    }

    public async Task<IReadOnlyList<BlogPostDto>> GetAllPostsAsync(CancellationToken ct = default)
    {
        var posts = await _blogPostRepository.GetAllAsync(ct);
        return posts.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<BlogPostDto>> GetAllPostsPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _blogPostRepository.GetPagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<BlogPostDto>.Create(items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<BlogPostDto> CreatePostAsync(BlogPostCreateDto dto, CancellationToken ct = default)
    {
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            Content = _htmlSanitizer.Sanitize(dto.Content),
            CoverImageUrl = dto.CoverImageUrl,
            Tags = dto.Tags,
            IsPublished = dto.IsPublished,
            PublishedAt = dto.IsPublished ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        await _blogPostRepository.AddAsync(post, ct);
        await _cache.RemoveAsync(CacheKeys.BlogPostsPublished, ct);
        return MapToDto(post);
    }

    public async Task<BlogPostDto> UpdatePostAsync(Guid id, BlogPostUpdateDto dto, CancellationToken ct = default)
    {
        var post = await _blogPostRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Blog post with ID {id} not found.");

        var wasPublished = post.IsPublished;

        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Summary = dto.Summary;
        post.Content = _htmlSanitizer.Sanitize(dto.Content);
        post.CoverImageUrl = dto.CoverImageUrl;
        post.Tags = dto.Tags;
        post.IsPublished = dto.IsPublished;
        post.UpdatedAt = DateTime.UtcNow;

        if (!wasPublished && dto.IsPublished)
            post.PublishedAt = DateTime.UtcNow;

        await _blogPostRepository.UpdateAsync(post, ct);
        await _cache.RemoveAsync(CacheKeys.BlogPostsPublished, ct);
        await _cache.RemoveAsync(CacheKeys.BlogPostBySlug(dto.Slug), ct);
        return MapToDto(post);
    }

    public async Task DeletePostAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _blogPostRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Blog post with ID {id} not found.");

        await _blogPostRepository.DeleteAsync(post, ct);
        await _cache.RemoveAsync(CacheKeys.BlogPostsPublished, ct);
        await _cache.RemoveAsync(CacheKeys.BlogPostBySlug(post.Slug), ct);
    }

    public async Task PublishPostAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _blogPostRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Blog post with ID {id} not found.");

        if (!post.IsPublished)
        {
            post.IsPublished = true;
            post.PublishedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await _blogPostRepository.UpdateAsync(post, ct);
            await _cache.RemoveAsync(CacheKeys.BlogPostsPublished, ct);
            await _cache.RemoveAsync(CacheKeys.BlogPostBySlug(post.Slug), ct);
        }
    }

    public async Task UnpublishPostAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _blogPostRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Blog post with ID {id} not found.");

        if (post.IsPublished)
        {
            post.IsPublished = false;
            post.UpdatedAt = DateTime.UtcNow;

            await _blogPostRepository.UpdateAsync(post, ct);
            await _cache.RemoveAsync(CacheKeys.BlogPostsPublished, ct);
            await _cache.RemoveAsync(CacheKeys.BlogPostBySlug(post.Slug), ct);
        }
    }

    private static BlogPostDto MapToDto(BlogPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Summary = p.Summary,
        Content = p.Content,
        CoverImageUrl = p.CoverImageUrl,
        Tags = p.Tags,
        IsPublished = p.IsPublished,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt
    };
}
