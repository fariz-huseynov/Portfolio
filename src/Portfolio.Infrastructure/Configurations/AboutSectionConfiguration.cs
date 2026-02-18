using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class AboutSectionConfiguration : IEntityTypeConfiguration<AboutSection>
{
    public void Configure(EntityTypeBuilder<AboutSection> builder)
    {
        builder.ToTable("AboutSections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.ProfileImageUrl).HasMaxLength(2000);
        builder.Property(e => e.ResumeUrl).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
