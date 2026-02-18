using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs;

public class LeadSubmitDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string CaptchaId { get; set; } = string.Empty;

    [Required]
    public string CaptchaCode { get; set; } = string.Empty;
}
