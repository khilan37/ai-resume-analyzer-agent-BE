using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartResumeAnalyzerAgent.Domain.Entities;

namespace SmartResumeAnalyzerAgent.Infrastructure.Persistence.Configurations;

public sealed class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.ToTable("resume_analysis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Summary).HasColumnType("text").IsRequired();
        builder.Property(x => x.OverallFeedback).HasColumnType("text").IsRequired();
        builder.Property(x => x.RawAiResponse).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Skills).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.MissingSkills).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Suggestions).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.MatchedJobRoles).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => x.Score);
        builder.HasIndex(x => x.Skills).HasMethod("gin");
        builder.HasIndex(x => x.MissingSkills).HasMethod("gin");
    }
}
