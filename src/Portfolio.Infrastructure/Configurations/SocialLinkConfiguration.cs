using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("SocialLinks");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Platform).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.IconUrl).HasMaxLength(2000);
        builder.Property(e => e.IsVisible).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.SortOrder);
    }
}
