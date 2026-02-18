namespace Portfolio.Application.DTOs.Content;

public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Proficiency { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
}
