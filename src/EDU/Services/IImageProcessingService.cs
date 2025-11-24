namespace EDU.Services;

public interface IImageProcessingService
{
    Task<byte[]> CreateThumbnailAsync(Stream imageStream, int width, int height);
}
