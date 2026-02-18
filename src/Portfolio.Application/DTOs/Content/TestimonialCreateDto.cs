using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class TestimonialCreateDto
{
    [Required, MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AuthorTitle { get; set; }

    [MaxLength(200)]
    public string? AuthorCompany { get; set; }

    [MaxLength(2000)]
    public string? AuthorImageUrl { get; set; }

    [Required]
    public string Quote { get; set; } = string.Empty;

    [Range(1, 5)]
    public int? Rating { get; set; }

    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
}
