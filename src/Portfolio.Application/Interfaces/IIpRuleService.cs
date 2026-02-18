using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Security;

namespace Portfolio.Application.Interfaces;

public interface IIpRuleService
{
    Task<PagedResult<IpRuleDto>> GetRulesPagedAsync(IpRuleFilterParams filter, CancellationToken ct = default);
    Task<IpRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken ct = default);
    Task<IpRuleDto> CreateRuleAsync(IpRuleCreateDto dto, string createdBy, CancellationToken ct = default);
    Task<IpRuleDto> UpdateRuleAsync(Guid id, IpRuleUpdateDto dto, CancellationToken ct = default);
    Task DeleteRuleAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsIpBlacklistedAsync(string ipAddress, CancellationToken ct = default);
    Task<bool> IsIpWhitelistedAsync(string ipAddress, CancellationToken ct = default);
    Task<HashSet<string>> GetActiveBlacklistedIpsAsync(CancellationToken ct = default);
    Task<HashSet<string>> GetActiveWhitelistedIpsAsync(CancellationToken ct = default);
}
