using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartResumeAnalyzerAgent.Application.Abstractions.AI;
using SmartResumeAnalyzerAgent.Application.DTOs.Resume;
using SmartResumeAnalyzerAgent.Application.Prompts;

namespace SmartResumeAnalyzerAgent.Infrastructure.AI;

public sealed class GeminiSemanticKernelResumeAnalyzerService : IAiResumeAnalyzerService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiSemanticKernelResumeAnalyzerService(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<(AiResumeAnalysisResult Result, string RawResponse)> AnalyzeAsync(string resumeText, string? jobRole, CancellationToken cancellationToken)
    {
        var prompt = RenderPrompt(resumeText, jobRole);
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.ModelId}:generateContent?key={_options.ApiKey}";

        var request = new GeminiGenerateContentRequest(
            new[]
            {
                new GeminiContent(
                    "user",
                    new[]
                    {
                        new GeminiPart(prompt)
                    })
            },
            new GeminiGenerationConfig("application/json"));

        using var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Gemini response was empty.");

        var raw = payload.Candidates?
            .SelectMany(x => x.Content?.Parts ?? Array.Empty<GeminiPart>())
            .Select(x => x.Text)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? "{}";

        var result = JsonSerializer.Deserialize<AiResumeAnalysisResult>(raw, JsonOptions) ?? new AiResumeAnalysisResult();
        result.Score = Math.Clamp(result.Score, 0, 100);
        result.Skills = Distinct(result.Skills);
        result.MissingSkills = Distinct(result.MissingSkills);
        result.Suggestions = Distinct(result.Suggestions);
        result.MatchedJobRoles = Distinct(result.MatchedJobRoles);

        return (result, raw);
    }

    private static string RenderPrompt(string resumeText, string? jobRole)
    {
        var tokens = new Dictionary<string, string>
        {
            ["{{$resumeText}}"] = resumeText,
            ["{{$jobRole}}"] = string.IsNullOrWhiteSpace(jobRole) ? "Not provided" : jobRole
        };

        return tokens.Aggregate(ResumeAnalysisPromptTemplate.Template, (current, pair) => current.Replace(pair.Key, pair.Value, StringComparison.Ordinal));
    }

    private static List<string> Distinct(IEnumerable<string>? values) =>
        values?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? new List<string>();

    private sealed record GeminiGenerateContentRequest(IReadOnlyCollection<GeminiContent> Contents, GeminiGenerationConfig GenerationConfig);
    private sealed record GeminiContent(string Role, IReadOnlyCollection<GeminiPart> Parts);
    private sealed record GeminiPart(string Text);
    private sealed record GeminiGenerationConfig(string ResponseMimeType);
    private sealed record GeminiGenerateContentResponse(IReadOnlyCollection<GeminiCandidate>? Candidates);
    private sealed record GeminiCandidate(GeminiContent? Content);
}
