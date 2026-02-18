namespace Portfolio.Application.DTOs.Content;

public class HeroSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? CtaText { get; set; }
    public string? CtaUrl { get; set; }
}
