namespace SmartResumeAnalyzerAgent.Infrastructure.AI;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gemini-1.5-pro";
}
