using SmartResumeAnalyzerAgent.Domain.Entities;

namespace SmartResumeAnalyzerAgent.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
