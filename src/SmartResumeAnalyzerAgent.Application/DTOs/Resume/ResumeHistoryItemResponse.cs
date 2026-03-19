namespace SmartResumeAnalyzerAgent.Application.DTOs.Resume;

public sealed class ResumeHistoryItemResponse
{
    public Guid ResumeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? JobRole { get; set; }
    public int Score { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public IReadOnlyCollection<string> Skills { get; set; } = Array.Empty<string>();
}
