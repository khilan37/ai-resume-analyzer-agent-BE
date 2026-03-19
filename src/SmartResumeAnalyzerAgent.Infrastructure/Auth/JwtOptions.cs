namespace SmartResumeAnalyzerAgent.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "SmartResumeAnalyzerAgent";
    public string Audience { get; set; } = "SmartResumeAnalyzerAgentClient";
    public string SecretKey { get; set; } = "ReplaceThisWithALongDevelopmentSecretKey123!";
    public int ExpirationMinutes { get; set; } = 120;
}
