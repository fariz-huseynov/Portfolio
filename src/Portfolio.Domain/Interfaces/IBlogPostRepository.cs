using Portfolio.Domain.Entities;

namespace Portfolio.Domain.Interfaces;

public interface IBlogPostRepository : IRepository<BlogPost>
{
    Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<BlogPost>> GetPublishedAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
