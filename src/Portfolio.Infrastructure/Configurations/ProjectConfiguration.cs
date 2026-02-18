using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Summary).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.ThumbnailUrl).HasMaxLength(2000);
        builder.Property(e => e.LiveUrl).HasMaxLength(2000);
        builder.Property(e => e.GitHubUrl).HasMaxLength(2000);
        builder.Property(e => e.TechStack).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.IsPublished).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.IsPublished);
        builder.HasIndex(e => e.SortOrder);
    }
}
