using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzerAgent.Application.Abstractions.Persistence;
using SmartResumeAnalyzerAgent.Domain.Entities;

namespace SmartResumeAnalyzerAgent.Infrastructure.Persistence.Repositories;

public sealed class ResumeRepository : IResumeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ResumeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Resume resume, CancellationToken cancellationToken)
    {
        await _dbContext.Resumes.AddAsync(resume, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Resume?> GetByIdAsync(Guid resumeId, CancellationToken cancellationToken) =>
        await _dbContext.Resumes
            .Include(x => x.Analysis)
            .FirstOrDefaultAsync(x => x.Id == resumeId, cancellationToken);

    public async Task<IReadOnlyList<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.Resumes
            .Include(x => x.Analysis)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
