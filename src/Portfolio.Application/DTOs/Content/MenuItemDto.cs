namespace Portfolio.Application.DTOs.Content;

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public Guid? ParentId { get; set; }
    public IReadOnlyList<MenuItemDto> Children { get; set; } = [];
}
