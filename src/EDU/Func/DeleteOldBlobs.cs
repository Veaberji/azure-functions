using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Services;

namespace EDU.Func;

public class DeleteOldBlobs(ILogger<DeleteOldBlobs> logger, IBlobStorageService blobStorageService)
{
    private const string EveryMinuteSchedule = "0 * * * * *";
    private const int MaxAgeSeconds = 120;

    [Function(nameof(DeleteOldBlobs))]
    public async Task Run([TimerTrigger(EveryMinuteSchedule)] TimerInfo timer)
    {
        if (timer.IsPastDue)
        {
            logger.LogWarning("Timer function is running late!");
        }

        logger.LogInformation("Timer Trigger function executed at: {Now}. Next run: {Next}", DateTime.Now, timer.ScheduleStatus?.Next);

        var maxAge = TimeSpan.FromSeconds(MaxAgeSeconds);

        await blobStorageService.DeleteOldBlobsAsync("image-input", maxAge);
    }
}