namespace EDU.Services;

public interface IBlobStorageService
{
    Task DeleteOldBlobsAsync(string containerName, TimeSpan maxAge);
}