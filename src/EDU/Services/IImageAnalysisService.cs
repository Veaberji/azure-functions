using EDU.Models;

namespace EDU.Services;

public interface IImageAnalysisService
{
    Task<IEnumerable<ImageTag>> AnalyzeImageTagsAsync(Stream imageStream);
}