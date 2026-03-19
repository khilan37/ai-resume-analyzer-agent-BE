using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartResumeAnalyzerAgent.Domain.Entities;

namespace SmartResumeAnalyzerAgent.Infrastructure.Persistence.Configurations;

public sealed class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("resumes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(400).IsRequired();
        builder.Property(x => x.TargetJobRole).HasMaxLength(120);
        builder.Property(x => x.ExtractedText).HasColumnType("text").IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasOne(x => x.User)
            .WithMany(x => x.Resumes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Analysis)
            .WithOne(x => x.Resume)
            .HasForeignKey<ResumeAnalysis>(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
