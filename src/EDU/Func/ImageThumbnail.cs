using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Services;

namespace EDU.Func;

public class ImageThumbnail(IImageProcessingService imageProcessingService, ILogger<ImageThumbnail> logger)
{
    [Function(nameof(ImageThumbnail))]
    [BlobOutput("image-output/thumbnail-{name}", Connection = "forfuncs_STORAGE")]
    public async Task<byte[]> Run([BlobTrigger("image-input/{name}", Connection = "forfuncs_STORAGE")] ReadOnlyMemory<byte> inBlob, string name)
    {
        try
        {
            using var stream = new MemoryStream(inBlob.ToArray());
            var thumbnail = await imageProcessingService.CreateThumbnailAsync(stream, 100, 100);
            logger.LogInformation("Thumbnail created for {Name}", name);
            return thumbnail;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing image {Name}", name);
            throw;
        }
    }
}