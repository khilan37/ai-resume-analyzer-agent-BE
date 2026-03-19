using SmartResumeAnalyzerAgent.Application.DTOs.Resume;

namespace SmartResumeAnalyzerAgent.Application.Abstractions.AI;

public interface IAiResumeAnalyzerService
{
    Task<(AiResumeAnalysisResult Result, string RawResponse)> AnalyzeAsync(string resumeText, string? jobRole, CancellationToken cancellationToken);
}
