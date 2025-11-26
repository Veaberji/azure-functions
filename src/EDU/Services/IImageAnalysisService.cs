namespace EDU.Services;

public interface IImageAnalysisService
{
    Task<IEnumerable<(string Name, float Confidence)>> AnalyzeImageTagsAsync(Stream imageStream);
}