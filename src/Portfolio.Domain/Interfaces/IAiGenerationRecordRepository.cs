using Portfolio.Domain.Entities;

namespace Portfolio.Domain.Interfaces;

public interface IAiGenerationRecordRepository : IRepository<AiGenerationRecord>
{
    Task<(IReadOnlyList<AiGenerationRecord> Items, int TotalCount)> GetByUserPagedAsync(
        string userId, int page, int pageSize, CancellationToken ct = default);

    Task<(IReadOnlyList<AiGenerationRecord> Items, int TotalCount)> GetByOperationTypePagedAsync(
        AiOperationType operationType, int page, int pageSize, CancellationToken ct = default);
}
