namespace Portfolio.Domain.Entities;

public class SiteSettings : BaseEntity
{
    public string SiteName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FooterText { get; set; }
}
