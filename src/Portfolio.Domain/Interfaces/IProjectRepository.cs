using Portfolio.Domain.Entities;

namespace Portfolio.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetPublishedAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
