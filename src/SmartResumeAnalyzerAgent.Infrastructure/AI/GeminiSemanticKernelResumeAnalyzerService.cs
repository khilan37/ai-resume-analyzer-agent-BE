using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SmartResumeAnalyzerAgent.Application.Abstractions.AI;
using SmartResumeAnalyzerAgent.Application.DTOs.Resume;
using SmartResumeAnalyzerAgent.Application.Prompts;

namespace SmartResumeAnalyzerAgent.Infrastructure.AI;

public sealed class GeminiSemanticKernelResumeAnalyzerService : IAiResumeAnalyzerService
{
    private readonly Kernel _kernel;

    public GeminiSemanticKernelResumeAnalyzerService(IOptions<GeminiOptions> options)
    {
        var gemini = options.Value;
        var builder = Kernel.CreateBuilder();

        // Adjust the connector registration if your Semantic Kernel Gemini package version uses a different extension name.
        builder.AddGoogleAIGeminiChatCompletion(gemini.ModelId, gemini.ApiKey);
        _kernel = builder.Build();
    }

    public async Task<(AiResumeAnalysisResult Result, string RawResponse)> AnalyzeAsync(string resumeText, string? jobRole, CancellationToken cancellationToken)
    {
        var arguments = new KernelArguments
        {
            ["resumeText"] = resumeText,
            ["jobRole"] = string.IsNullOrWhiteSpace(jobRole) ? "Not provided" : jobRole
        };

        var response = await _kernel.InvokePromptAsync(ResumeAnalysisPromptTemplate.Template, arguments, cancellationToken: cancellationToken);
        var raw = response.ToString() ?? "{}";

        var result = JsonSerializer.Deserialize<AiResumeAnalysisResult>(
            raw,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AiResumeAnalysisResult();

        result.Score = Math.Clamp(result.Score, 0, 100);
        result.Skills = Distinct(result.Skills);
        result.MissingSkills = Distinct(result.MissingSkills);
        result.Suggestions = Distinct(result.Suggestions);
        result.MatchedJobRoles = Distinct(result.MatchedJobRoles);

        return (result, raw);
    }

    private static List<string> Distinct(IEnumerable<string>? values) =>
        values?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? new List<string>();
}
