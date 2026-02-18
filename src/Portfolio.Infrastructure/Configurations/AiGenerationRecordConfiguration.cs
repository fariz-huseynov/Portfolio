using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public class AiGenerationRecordConfiguration : IEntityTypeConfiguration<AiGenerationRecord>
{
    public void Configure(EntityTypeBuilder<AiGenerationRecord> builder)
    {
        builder.ToTable("AiGenerationRecords");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Provider).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.OperationType).IsRequired()
            .HasConversion<string>().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired()
            .HasConversion<string>().HasMaxLength(50);

        builder.Property(e => e.Prompt).IsRequired();
        builder.Property(e => e.SystemPrompt).HasMaxLength(5000);
        builder.Property(e => e.ModelName).HasMaxLength(100);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.ResultImageUrl).HasMaxLength(2000);
        builder.Property(e => e.RequestedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => e.CreatedAt).IsDescending();
    }
}
