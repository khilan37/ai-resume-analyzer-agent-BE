using SmartResumeAnalyzerAgent.Application.Abstractions.AI;
using SmartResumeAnalyzerAgent.Application.Abstractions.Files;
using SmartResumeAnalyzerAgent.Application.Abstractions.Persistence;
using SmartResumeAnalyzerAgent.Application.DTOs.Resume;
using SmartResumeAnalyzerAgent.Domain.Entities;
using SmartResumeAnalyzerAgent.Domain.Enums;

namespace SmartResumeAnalyzerAgent.Application.Features.Resumes;

public sealed class ResumeService
{
    private readonly IUserRepository _userRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IResumeTextExtractor _resumeTextExtractor;
    private readonly IAiResumeAnalyzerService _aiResumeAnalyzerService;

    public ResumeService(
        IUserRepository userRepository,
        IResumeRepository resumeRepository,
        IResumeTextExtractor resumeTextExtractor,
        IAiResumeAnalyzerService aiResumeAnalyzerService)
    {
        _userRepository = userRepository;
        _resumeRepository = resumeRepository;
        _resumeTextExtractor = resumeTextExtractor;
        _aiResumeAnalyzerService = aiResumeAnalyzerService;
    }

    public async Task<ResumeAnalysisResponse> UploadAndAnalyzeAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        ResumeUploadRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var extractedText = await _resumeTextExtractor.ExtractTextAsync(fileStream, fileName, cancellationToken);
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException("No text could be extracted from the uploaded file.");
        }

        var resume = new Resume
        {
            UserId = user.Id,
            FileName = fileName,
            ContentType = contentType,
            StoragePath = $"uploads/{Guid.NewGuid()}-{fileName}",
            ExtractedText = extractedText,
            TargetJobRole = request.JobRole,
            Status = ResumeProcessingStatus.Processing
        };

        await _resumeRepository.AddAsync(resume, cancellationToken);

        var (result, rawResponse) = await _aiResumeAnalyzerService.AnalyzeAsync(extractedText, request.JobRole, cancellationToken);
        resume.Status = ResumeProcessingStatus.Completed;
        resume.Analysis = new ResumeAnalysis
        {
            ResumeId = resume.Id,
            Score = result.Score,
            Summary = result.Summary,
            OverallFeedback = result.OverallFeedback,
            Skills = result.Skills,
            MissingSkills = result.MissingSkills,
            Suggestions = result.Suggestions,
            MatchedJobRoles = result.MatchedJobRoles,
            RawAiResponse = rawResponse
        };

        await _resumeRepository.SaveChangesAsync(cancellationToken);
        return Map(resume);
    }

    public async Task<ResumeAnalysisResponse?> GetAnalysisAsync(Guid resumeId, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, cancellationToken);
        return resume is null || resume.Analysis is null ? null : Map(resume);
    }

    public async Task<IReadOnlyCollection<ResumeHistoryItemResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var resumes = await _resumeRepository.GetByUserIdAsync(userId, cancellationToken);
        return resumes
            .Where(x => x.Analysis is not null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ResumeHistoryItemResponse
            {
                ResumeId = x.Id,
                FileName = x.FileName,
                JobRole = x.TargetJobRole,
                Score = x.Analysis!.Score,
                UploadedAtUtc = x.CreatedAtUtc,
                Skills = x.Analysis.Skills.Take(6).ToArray()
            })
            .ToArray();
    }

    private static ResumeAnalysisResponse Map(Resume resume)
    {
        var analysis = resume.Analysis ?? throw new InvalidOperationException("Resume analysis is not available.");
        return new ResumeAnalysisResponse
        {
            ResumeId = resume.Id,
            FileName = resume.FileName,
            JobRole = resume.TargetJobRole,
            Score = analysis.Score,
            Summary = analysis.Summary,
            OverallFeedback = analysis.OverallFeedback,
            Skills = analysis.Skills,
            MissingSkills = analysis.MissingSkills,
            Suggestions = analysis.Suggestions,
            MatchedJobRoles = analysis.MatchedJobRoles,
            CreatedAtUtc = resume.CreatedAtUtc
        };
    }
}
