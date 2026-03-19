namespace SmartResumeAnalyzerAgent.Application.DTOs.Resume;

public sealed class ResumeUploadRequest
{
    public Guid UserId { get; set; }
    public string? JobRole { get; set; }
}
