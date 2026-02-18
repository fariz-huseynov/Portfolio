using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Infrastructure.Data;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<Project?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<IReadOnlyList<Project>> GetPublishedAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPublishedPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderBy(p => p.SortOrder);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
