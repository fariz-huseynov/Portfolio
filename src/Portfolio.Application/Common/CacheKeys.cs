namespace Portfolio.Application.Common;

public static class CacheKeys
{
    public const string SiteSettings = "site:settings";
    public const string HeroSection = "site:hero";
    public const string AboutSection = "site:about";
    public const string Skills = "site:skills";
    public const string Experiences = "site:experiences";
    public const string Services = "site:services";
    public const string Testimonials = "site:testimonials:published";
    public const string SocialLinks = "site:sociallinks:visible";
    public const string MenuItems = "site:menu:visible";

    public static string BlogPostBySlug(string slug) => $"blog:slug:{slug}";
    public static string BlogPostsPublished => "blog:published";

    public static string ProjectBySlug(string slug) => $"project:slug:{slug}";
    public static string ProjectsPublished => "project:published";

    public static string IpRulesBlacklist => "iprules:blacklist";
    public static string IpRulesWhitelist => "iprules:whitelist";

    public static string Permissions(IEnumerable<string> roleNames)
        => $"permissions:{string.Join(",", roleNames.OrderBy(r => r))}";

    public static string AiGeneration(Guid id) => $"ai:generation:{id}";
}
