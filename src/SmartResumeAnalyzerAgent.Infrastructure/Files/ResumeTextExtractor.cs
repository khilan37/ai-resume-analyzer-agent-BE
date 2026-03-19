using System.Text;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using SmartResumeAnalyzerAgent.Application.Abstractions.Files;

namespace SmartResumeAnalyzerAgent.Infrastructure.Files;

public sealed class ResumeTextExtractor : IResumeTextExtractor
{
    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => await ExtractPdfAsync(fileStream, cancellationToken),
            ".docx" => await ExtractDocxAsync(fileStream, cancellationToken),
            _ => throw new NotSupportedException("Only PDF and DOCX files are supported.")
        };
    }

    private static Task<string> ExtractPdfAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var memory = new MemoryStream();
        fileStream.CopyTo(memory);
        memory.Position = 0;

        using var reader = new PdfReader(memory);
        using var pdf = new PdfDocument(reader);
        var builder = new StringBuilder();
        for (var page = 1; page <= pdf.GetNumberOfPages(); page++)
        {
            builder.AppendLine(PdfTextExtractor.GetTextFromPage(pdf.GetPage(page)));
        }

        return Task.FromResult(builder.ToString());
    }

    private static Task<string> ExtractDocxAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var memory = new MemoryStream();
        fileStream.CopyTo(memory);
        memory.Position = 0;

        using var document = WordprocessingDocument.Open(memory, false);
        var text = document.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        return Task.FromResult(text);
    }
}
