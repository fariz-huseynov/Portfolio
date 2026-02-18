using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Infrastructure.Data;

public class IpRuleRepository : Repository<IpRule>, IIpRuleRepository
{
    public IpRuleRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<IpRule>> GetActiveRulesByTypeAsync(IpRuleType ruleType, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Where(r => r.RuleType == ruleType && r.IsActive)
            .ToListAsync(ct);

    public async Task<IpRule?> GetByIpAddressAsync(string ipAddress, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(r => r.IpAddress == ipAddress, ct);

    public async Task<(IReadOnlyList<IpRule> Items, int TotalCount)> GetFilteredPagedAsync(
        IpRuleType? ruleType, string? searchText, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (ruleType.HasValue)
            query = query.Where(r => r.RuleType == ruleType.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(r => r.IpAddress.Contains(searchText) ||
                (r.Reason != null && r.Reason.Contains(searchText)));

        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
