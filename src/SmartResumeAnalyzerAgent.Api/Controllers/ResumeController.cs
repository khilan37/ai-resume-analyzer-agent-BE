using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzerAgent.Api.Models;
using SmartResumeAnalyzerAgent.Application.DTOs.Resume;
using SmartResumeAnalyzerAgent.Application.Features.Resumes;

namespace SmartResumeAnalyzerAgent.Api.Controllers;

[ApiController]
[Route("api/resumes")]
public sealed class ResumeController : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly ResumeService _resumeService;

    public ResumeController(ResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpPost("analyze")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<ResumeAnalysisResponse>> Analyze([FromForm] AnalyzeResumeFormRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        if (!AllowedContentTypes.Contains(request.File.ContentType))
        {
            return BadRequest("Only PDF and DOCX files are supported.");
        }

        await using var stream = request.File.OpenReadStream();
        var response = await _resumeService.UploadAndAnalyzeAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            new ResumeUploadRequest
            {
                UserId = request.UserId,
                JobRole = request.JobRole
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{resumeId:guid}")]
    public async Task<ActionResult<ResumeAnalysisResponse>> GetById(Guid resumeId, CancellationToken cancellationToken)
    {
        var response = await _resumeService.GetAnalysisAsync(resumeId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("history/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<ResumeHistoryItemResponse>>> GetHistory(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _resumeService.GetHistoryAsync(userId, cancellationToken);
        return Ok(response);
    }
}
