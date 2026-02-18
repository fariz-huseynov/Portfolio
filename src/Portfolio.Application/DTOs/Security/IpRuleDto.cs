namespace Portfolio.Application.DTOs.Security;

public class IpRuleDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
