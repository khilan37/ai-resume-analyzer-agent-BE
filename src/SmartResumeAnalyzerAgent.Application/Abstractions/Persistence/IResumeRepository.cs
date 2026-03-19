using SmartResumeAnalyzerAgent.Domain.Entities;

namespace SmartResumeAnalyzerAgent.Application.Abstractions.Persistence;

public interface IResumeRepository
{
    Task AddAsync(Resume resume, CancellationToken cancellationToken);
    Task<Resume?> GetByIdAsync(Guid resumeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
