namespace Portfolio.Domain.Entities;

public class AboutSection : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? ResumeUrl { get; set; }
}
