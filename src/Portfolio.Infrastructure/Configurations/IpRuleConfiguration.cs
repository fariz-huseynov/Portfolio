using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class IpRuleConfiguration : IEntityTypeConfiguration<IpRule>
{
    public void Configure(EntityTypeBuilder<IpRule> builder)
    {
        builder.ToTable("IpRules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(e => e.RuleType).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.IpAddress);
        builder.HasIndex(e => new { e.RuleType, e.IsActive });
    }
}
