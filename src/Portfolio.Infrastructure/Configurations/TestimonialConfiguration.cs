using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.AuthorName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.AuthorTitle).HasMaxLength(200);
        builder.Property(e => e.AuthorCompany).HasMaxLength(200);
        builder.Property(e => e.AuthorImageUrl).HasMaxLength(2000);
        builder.Property(e => e.Quote).IsRequired();
        builder.Property(e => e.IsPublished).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.IsPublished);
        builder.HasIndex(e => e.SortOrder);
    }
}
