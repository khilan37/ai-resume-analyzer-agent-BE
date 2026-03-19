using SmartResumeAnalyzerAgent.Domain.Common;

namespace SmartResumeAnalyzerAgent.Domain.Entities;

public sealed class ResumeAnalysis : BaseEntity
{
    public Guid ResumeId { get; set; }
    public int Score { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string OverallFeedback { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public List<string> MatchedJobRoles { get; set; } = new();
    public string RawAiResponse { get; set; } = "{}";
    public Resume? Resume { get; set; }
}
