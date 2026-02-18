using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class ServiceCreateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
}
