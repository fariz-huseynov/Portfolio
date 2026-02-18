namespace Portfolio.Domain.Entities;

public class IpRule : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    public IpRuleType RuleType { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
}
