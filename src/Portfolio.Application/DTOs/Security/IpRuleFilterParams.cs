using Portfolio.Application.DTOs.Pagination;

namespace Portfolio.Application.DTOs.Security;

public class IpRuleFilterParams : PaginationParams
{
    public string? RuleType { get; set; }
    public string? SearchText { get; set; }
}
