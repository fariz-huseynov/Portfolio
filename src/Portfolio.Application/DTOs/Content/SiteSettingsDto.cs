namespace Portfolio.Application.DTOs.Content;

public class SiteSettingsDto
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FooterText { get; set; }
}
