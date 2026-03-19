using SmartResumeAnalyzerAgent.Domain.Common;
using SmartResumeAnalyzerAgent.Domain.Enums;

namespace SmartResumeAnalyzerAgent.Domain.Entities;

public sealed class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public string? TargetJobRole { get; set; }
    public ResumeProcessingStatus Status { get; set; } = ResumeProcessingStatus.Uploaded;
    public User? User { get; set; }
    public ResumeAnalysis? Analysis { get; set; }
}
