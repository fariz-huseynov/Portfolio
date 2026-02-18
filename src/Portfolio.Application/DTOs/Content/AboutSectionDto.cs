namespace Portfolio.Application.DTOs.Content;

public class AboutSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? ResumeUrl { get; set; }
}
