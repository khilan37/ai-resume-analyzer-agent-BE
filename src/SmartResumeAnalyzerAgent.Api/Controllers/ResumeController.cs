using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzerAgent.Application.DTOs.Resume;
using SmartResumeAnalyzerAgent.Application.Features.Resumes;

namespace SmartResumeAnalyzerAgent.Api.Controllers;

[ApiController]
[Authorize]
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
    public async Task<ActionResult<ResumeAnalysisResponse>> Analyze([FromForm] IFormFile file, [FromForm] Guid userId, [FromForm] string? jobRole, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest("Only PDF and DOCX files are supported.");
        }

        await using var stream = file.OpenReadStream();
        var response = await _resumeService.UploadAndAnalyzeAsync(
            stream,
            file.FileName,
            file.ContentType,
            new ResumeUploadRequest
            {
                UserId = userId,
                JobRole = jobRole
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
