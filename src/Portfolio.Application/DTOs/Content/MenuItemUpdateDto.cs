using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.DTOs.Content;

public class MenuItemUpdateDto
{
    [Required, MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public Guid? ParentId { get; set; }
}
