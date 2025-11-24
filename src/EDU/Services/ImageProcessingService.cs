using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EDU.Services;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<byte[]> CreateThumbnailAsync(Stream imageStream, int width, int height)
    {
        using var image = await Image.LoadAsync(imageStream);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
}