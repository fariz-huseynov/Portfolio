using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class HeroSectionConfiguration : IEntityTypeConfiguration<HeroSection>
{
    public void Configure(EntityTypeBuilder<HeroSection> builder)
    {
        builder.ToTable("HeroSections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Subtitle).HasMaxLength(500);
        builder.Property(e => e.BackgroundImageUrl).HasMaxLength(2000);
        builder.Property(e => e.CtaText).HasMaxLength(100);
        builder.Property(e => e.CtaUrl).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
