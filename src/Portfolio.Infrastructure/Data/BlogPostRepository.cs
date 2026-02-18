using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Infrastructure.Data;

public class BlogPostRepository : Repository<BlogPost>, IBlogPostRepository
{
    public BlogPostRepository(AppDbContext context) : base(context) { }

    public async Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == slug, ct);

    public async Task<IReadOnlyList<BlogPost>> GetPublishedAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedAt)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
