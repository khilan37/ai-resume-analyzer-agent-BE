using SmartResumeAnalyzerAgent.Domain.Common;

namespace SmartResumeAnalyzerAgent.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
