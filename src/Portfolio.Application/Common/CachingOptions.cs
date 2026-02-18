namespace Portfolio.Application.Common;

public class CachingOptions
{
    public int SiteContentMinutes { get; set; } = 10;
    public int BlogPostMinutes { get; set; } = 30;
    public int ProjectMinutes { get; set; } = 30;
    public int PublishedListMinutes { get; set; } = 15;
    public int IpRulesMinutes { get; set; } = 5;
    public int PermissionsMinutes { get; set; } = 5;
}
