using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Infrastructure.Data;

public class AiGenerationRecordRepository : Repository<AiGenerationRecord>, IAiGenerationRecordRepository
{
    public AiGenerationRecordRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<AiGenerationRecord> Items, int TotalCount)> GetByUserPagedAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.RequestedByUserId == userId);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<AiGenerationRecord> Items, int TotalCount)> GetByOperationTypePagedAsync(
        AiOperationType operationType, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.OperationType == operationType);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
        return (items, totalCount);
    }
}
