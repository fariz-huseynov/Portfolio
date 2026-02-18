using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Infrastructure.Data;

public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Lead>> GetAllOrderedByDateAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetAllOrderedByDatePagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
