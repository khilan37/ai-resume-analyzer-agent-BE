namespace SmartResumeAnalyzerAgent.Application.Abstractions.Files;

public interface IResumeTextExtractor
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);
}
