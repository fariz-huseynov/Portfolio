using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class SiteSettingsUpdateDto
{
    [Required, MaxLength(200)]
    public string SiteName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? LogoUrl { get; set; }

    [MaxLength(2000)]
    public string? FaviconUrl { get; set; }

    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    [MaxLength(500)]
    public string? SeoKeywords { get; set; }

    [MaxLength(50)]
    public string? GoogleAnalyticsId { get; set; }

    [MaxLength(1000)]
    public string? FooterText { get; set; }
}
