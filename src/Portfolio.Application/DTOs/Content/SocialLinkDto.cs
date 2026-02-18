namespace Portfolio.Application.DTOs.Content;

public class SocialLinkDto
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
}
