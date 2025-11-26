namespace EDU.Models;

public class ImageAnalysisDocument
{
    public string Id { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public List<ImageTag> Tags { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}