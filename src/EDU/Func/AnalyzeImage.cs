using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;
using EDU.Services;
using EDU.Models;

namespace EDU.Func;

public class AnalyzeImage(ILogger<AnalyzeImage> logger, IImageAnalysisService imageAnalysisService)
{
    [Function(nameof(AnalyzeImage))]
    [CosmosDBOutput("TestDB", "TestCollection", Connection = "COSMOS_ENDPOINT", CreateIfNotExists = true)]
    public async Task<ImageAnalysisDocument> Run([BlobTrigger("analyze-image-input/{name}", Connection = "forfuncs_STORAGE")] Stream stream, string name)
    {
        logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name}", name);

        var tags = await imageAnalysisService.AnalyzeImageTagsAsync(stream);

        logger.LogInformation("Analysis complete. Tags: {tags}", string.Join(", ", tags.Select(t => t.Name)));

        var tagsList = tags.Select(t => new ImageTag { Name = t.Name, Confidence = t.Confidence }).ToList();
        var document = new ImageAnalysisDocument
        {
            Id = Guid.NewGuid().ToString(),
            ImageName = name,
            Tags = tagsList,
            AnalyzedAt = DateTime.UtcNow
        };

        return document;
    }
}