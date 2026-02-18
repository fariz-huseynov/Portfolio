using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;

namespace Portfolio.Application.Interfaces;

public interface IBlogPostService
{
    Task<IReadOnlyList<BlogPostDto>> GetPublishedPostsAsync(CancellationToken ct = default);
    Task<PagedResult<BlogPostDto>> GetPublishedPostsPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<BlogPostDto?> GetPostBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<BlogPostDto>> GetAllPostsAsync(CancellationToken ct = default);
    Task<PagedResult<BlogPostDto>> GetAllPostsPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<BlogPostDto> CreatePostAsync(BlogPostCreateDto dto, CancellationToken ct = default);
    Task<BlogPostDto> UpdatePostAsync(Guid id, BlogPostUpdateDto dto, CancellationToken ct = default);
    Task DeletePostAsync(Guid id, CancellationToken ct = default);
    Task PublishPostAsync(Guid id, CancellationToken ct = default);
    Task UnpublishPostAsync(Guid id, CancellationToken ct = default);
}
