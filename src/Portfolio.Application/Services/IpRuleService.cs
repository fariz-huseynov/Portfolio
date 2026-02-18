using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Security;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class IpRuleService : IIpRuleService
{
    private readonly IIpRuleRepository _ipRuleRepository;

    public IpRuleService(IIpRuleRepository ipRuleRepository)
    {
        _ipRuleRepository = ipRuleRepository;
    }

    public async Task<PagedResult<IpRuleDto>> GetRulesPagedAsync(IpRuleFilterParams filter, CancellationToken ct = default)
    {
        IpRuleType? ruleType = null;
        if (!string.IsNullOrWhiteSpace(filter.RuleType) && Enum.TryParse<IpRuleType>(filter.RuleType, true, out var parsed))
            ruleType = parsed;

        var (items, totalCount) = await _ipRuleRepository.GetFilteredPagedAsync(
            ruleType, filter.SearchText, filter.Page, filter.PageSize, ct);

        return PagedResult<IpRuleDto>.Create(
            items.Select(MapToDto).ToList(), filter.Page, filter.PageSize, totalCount);
    }

    public async Task<IpRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _ipRuleRepository.GetByIdAsync(id, ct);
        return rule is null ? null : MapToDto(rule);
    }

    public async Task<IpRuleDto> CreateRuleAsync(IpRuleCreateDto dto, string createdBy, CancellationToken ct = default)
    {
        if (!Enum.TryParse<IpRuleType>(dto.RuleType, true, out var ruleType))
            throw new ArgumentException($"Invalid rule type: {dto.RuleType}. Must be 'Blacklist' or 'Whitelist'.");

        var existing = await _ipRuleRepository.GetByIpAddressAsync(dto.IpAddress, ct);
        if (existing is not null)
            throw new InvalidOperationException($"An IP rule already exists for {dto.IpAddress}.");

        var rule = new IpRule
        {
            Id = Guid.NewGuid(),
            IpAddress = dto.IpAddress,
            RuleType = ruleType,
            Reason = dto.Reason,
            IsActive = dto.IsActive,
            ExpiresAt = dto.ExpiresAt,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _ipRuleRepository.AddAsync(rule, ct);
        return MapToDto(rule);
    }

    public async Task<IpRuleDto> UpdateRuleAsync(Guid id, IpRuleUpdateDto dto, CancellationToken ct = default)
    {
        var rule = await _ipRuleRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"IP rule with ID {id} not found.");

        if (!Enum.TryParse<IpRuleType>(dto.RuleType, true, out var ruleType))
            throw new ArgumentException($"Invalid rule type: {dto.RuleType}. Must be 'Blacklist' or 'Whitelist'.");

        var existing = await _ipRuleRepository.GetByIpAddressAsync(dto.IpAddress, ct);
        if (existing is not null && existing.Id != id)
            throw new InvalidOperationException($"An IP rule already exists for {dto.IpAddress}.");

        rule.IpAddress = dto.IpAddress;
        rule.RuleType = ruleType;
        rule.Reason = dto.Reason;
        rule.IsActive = dto.IsActive;
        rule.ExpiresAt = dto.ExpiresAt;
        rule.UpdatedAt = DateTime.UtcNow;

        await _ipRuleRepository.UpdateAsync(rule, ct);
        return MapToDto(rule);
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _ipRuleRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"IP rule with ID {id} not found.");

        await _ipRuleRepository.DeleteAsync(rule, ct);
    }

    public async Task<bool> IsIpBlacklistedAsync(string ipAddress, CancellationToken ct = default)
    {
        var rules = await _ipRuleRepository.GetActiveRulesByTypeAsync(IpRuleType.Blacklist, ct);
        return rules.Any(r => r.IpAddress == ipAddress &&
            (r.ExpiresAt is null || r.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<bool> IsIpWhitelistedAsync(string ipAddress, CancellationToken ct = default)
    {
        var rules = await _ipRuleRepository.GetActiveRulesByTypeAsync(IpRuleType.Whitelist, ct);
        return rules.Any(r => r.IpAddress == ipAddress &&
            (r.ExpiresAt is null || r.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<HashSet<string>> GetActiveBlacklistedIpsAsync(CancellationToken ct = default)
    {
        var rules = await _ipRuleRepository.GetActiveRulesByTypeAsync(IpRuleType.Blacklist, ct);
        return rules
            .Where(r => r.ExpiresAt is null || r.ExpiresAt > DateTime.UtcNow)
            .Select(r => r.IpAddress)
            .ToHashSet();
    }

    public async Task<HashSet<string>> GetActiveWhitelistedIpsAsync(CancellationToken ct = default)
    {
        var rules = await _ipRuleRepository.GetActiveRulesByTypeAsync(IpRuleType.Whitelist, ct);
        return rules
            .Where(r => r.ExpiresAt is null || r.ExpiresAt > DateTime.UtcNow)
            .Select(r => r.IpAddress)
            .ToHashSet();
    }

    private static IpRuleDto MapToDto(IpRule r) => new()
    {
        Id = r.Id,
        IpAddress = r.IpAddress,
        RuleType = r.RuleType.ToString(),
        Reason = r.Reason,
        IsActive = r.IsActive,
        ExpiresAt = r.ExpiresAt,
        CreatedBy = r.CreatedBy,
        CreatedAt = r.CreatedAt
    };
}
