namespace Portfolio.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Proficiency { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
}
