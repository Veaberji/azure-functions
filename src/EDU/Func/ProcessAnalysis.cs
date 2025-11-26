using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Services;
using EDU.Models;

namespace EDU.Func;

public class ProcessAnalysis(ILogger<ProcessAnalysis> logger, IEmailService emailService)
{

    [Function(nameof(ProcessAnalysis))]
    public async Task Run([CosmosDBTrigger("TestDB", "TestCollection", Connection = "forfuncs_COSMOS_DB")] IReadOnlyList<ImageAnalysisDocument> documents)
    {
        if (documents is null || documents.Count == 0)
        {
            return;
        }

        foreach (var document in documents)
        {
            logger.LogInformation("New analysis result added to CosmosDB with ID = {id}", document.Id);

            await emailService.SendAnalysisEmailAsync(document.Tags, document.ImageName);

            logger.LogInformation("Email sent for image: {imageName}", document.ImageName);
        }
    }
}