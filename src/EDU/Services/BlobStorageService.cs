using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace EDU.Services;

public class BlobStorageService(ILogger<BlobStorageService> logger) : IBlobStorageService
{
    public async Task DeleteOldBlobsAsync(string containerName, TimeSpan maxAge)
    {
        var connectionString = Environment.GetEnvironmentVariable("forfuncs_STORAGE");

        var containerClient = new BlobContainerClient(connectionString, containerName);

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var properties = await blobClient.GetPropertiesAsync();
            var age = DateTimeOffset.Now - properties.Value.CreatedOn;

            if (age > maxAge)
            {
                await blobClient.DeleteAsync();
                logger.LogInformation("Deleted old blob: {BlobName}", blobItem.Name);
            }
        }
    }
}