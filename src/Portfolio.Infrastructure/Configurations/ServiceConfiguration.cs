using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portfolio.Infrastructure.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Domain.Entities.Service>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Service> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.IconUrl).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.SortOrder);
    }
}
