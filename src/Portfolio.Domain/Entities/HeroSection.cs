namespace Portfolio.Domain.Entities;

public class HeroSection : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? CtaText { get; set; }
    public string? CtaUrl { get; set; }
}
