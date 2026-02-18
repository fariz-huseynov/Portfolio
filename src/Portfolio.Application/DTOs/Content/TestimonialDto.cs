namespace Portfolio.Application.DTOs.Content;

public class TestimonialDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorTitle { get; set; }
    public string? AuthorCompany { get; set; }
    public string? AuthorImageUrl { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
}
