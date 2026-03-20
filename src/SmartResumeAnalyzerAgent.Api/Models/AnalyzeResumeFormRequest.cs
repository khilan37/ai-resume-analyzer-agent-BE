namespace SmartResumeAnalyzerAgent.Api.Models;

public sealed class AnalyzeResumeFormRequest
{
    public IFormFile File { get; set; } = default!;
    public Guid UserId { get; set; }
    public string? JobRole { get; set; }
}
