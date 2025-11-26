using Azure.AI.Vision.ImageAnalysis;

namespace EDU.Services;

public class ImageAnalysisService(ImageAnalysisClient visionClient) : IImageAnalysisService
{
    public async Task<IEnumerable<(string Name, float Confidence)>> AnalyzeImageTagsAsync(Stream imageStream)
    {
        var response = await visionClient.AnalyzeAsync(BinaryData.FromStream(imageStream), VisualFeatures.Tags);
        return response.Value.Tags.Values.Select(t => (t.Name, t.Confidence));
    }
}