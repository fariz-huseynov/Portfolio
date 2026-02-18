using Portfolio.Domain.Entities;

namespace Portfolio.Domain.Interfaces;

public interface ILeadRepository : IRepository<Lead>
{
    Task<IReadOnlyList<Lead>> GetAllOrderedByDateAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetAllOrderedByDatePagedAsync(int page, int pageSize, CancellationToken ct = default);
}
