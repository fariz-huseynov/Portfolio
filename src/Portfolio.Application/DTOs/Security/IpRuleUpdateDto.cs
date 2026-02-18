using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Security;

public class IpRuleUpdateDto
{
    [Required, MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    public string RuleType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }
}
