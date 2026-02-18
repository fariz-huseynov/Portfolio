using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.SiteName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.LogoUrl).HasMaxLength(2000);
        builder.Property(e => e.FaviconUrl).HasMaxLength(2000);
        builder.Property(e => e.SeoTitle).HasMaxLength(200);
        builder.Property(e => e.SeoDescription).HasMaxLength(500);
        builder.Property(e => e.SeoKeywords).HasMaxLength(500);
        builder.Property(e => e.GoogleAnalyticsId).HasMaxLength(50);
        builder.Property(e => e.FooterText).HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
