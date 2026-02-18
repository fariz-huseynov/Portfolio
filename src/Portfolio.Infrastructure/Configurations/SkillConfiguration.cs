using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.IconUrl).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.SortOrder);
    }
}
