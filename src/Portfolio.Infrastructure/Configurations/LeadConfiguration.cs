using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Company).HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(500);
        builder.Property(e => e.IsRead).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.CreatedAt).IsDescending();
        builder.HasIndex(e => e.IsRead);
    }
}
