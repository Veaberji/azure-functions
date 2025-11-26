using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Services;
using EDU.Models;

namespace EDU.Func;

public class ProcessAnalysis(ILogger<ProcessAnalysis> logger, IEmailService emailService)
{

    [Function(nameof(ProcessAnalysis))]
    public async Task Run([CosmosDBTrigger("TestDB", "TestCollection", Connection = "COSMOS_ENDPOINT")] IReadOnlyList<ImageAnalysisDocument> documents)
    {
        if (documents is not null && documents.Count > 0)
        {
            foreach (var document in documents)
            {
                logger.LogInformation("New analysis result added to CosmosDB with ID = {id}", document.Id);

                var tags = document.Tags.Select(t => (t.Name, t.Confidence)).ToList();
                await emailService.SendAnalysisEmailAsync(tags, document.ImageName);

                logger.LogInformation("Email sent for image: {imageName}", document.ImageName);
            }
        }
    }
}