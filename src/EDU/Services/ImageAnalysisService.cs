using Azure.AI.Vision.ImageAnalysis;
using EDU.Models;

namespace EDU.Services;

public class ImageAnalysisService(ImageAnalysisClient visionClient) : IImageAnalysisService
{
    public async Task<IEnumerable<ImageTag>> AnalyzeImageTagsAsync(Stream imageStream)
    {
        var response = await visionClient.AnalyzeAsync(BinaryData.FromStream(imageStream), VisualFeatures.Tags);
        return response.Value.Tags.Values.Select(t => new ImageTag { Name = t.Name, Confidence = t.Confidence });
    }
}