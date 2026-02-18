using Portfolio.Domain.Entities;

namespace Portfolio.Domain.Interfaces;

public interface IIpRuleRepository : IRepository<IpRule>
{
    Task<IReadOnlyList<IpRule>> GetActiveRulesByTypeAsync(IpRuleType ruleType, CancellationToken ct = default);
    Task<IpRule?> GetByIpAddressAsync(string ipAddress, CancellationToken ct = default);
    Task<(IReadOnlyList<IpRule> Items, int TotalCount)> GetFilteredPagedAsync(
        IpRuleType? ruleType, string? searchText, int page, int pageSize, CancellationToken ct = default);
}
