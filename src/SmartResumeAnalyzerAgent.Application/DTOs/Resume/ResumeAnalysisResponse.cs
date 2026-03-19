namespace SmartResumeAnalyzerAgent.Application.DTOs.Resume;

public sealed class ResumeAnalysisResponse
{
    public Guid ResumeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? JobRole { get; set; }
    public int Score { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string OverallFeedback { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Skills { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> MissingSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Suggestions { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> MatchedJobRoles { get; set; } = Array.Empty<string>();
    public DateTime CreatedAtUtc { get; set; }
}
