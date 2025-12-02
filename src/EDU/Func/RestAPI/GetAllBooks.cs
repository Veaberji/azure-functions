using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Models;
using Microsoft.Azure.Cosmos;

namespace EDU.Func.RestAPI;

public class GetAllBooks(ILogger<GetAllBooks> logger, CosmosClient cosmosClient)
{
    [Function("GetAllBooks")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        logger.LogInformation("GetAllBooks Function Triggered");

        try
        {
            var database = cosmosClient.GetDatabase("afccosmosdatabase");
            var container = database.GetContainer("afccosmoscollection");
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = container.GetItemQueryIterator<Book>(query);
            var results = new List<Book>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving books");
            return new StatusCodeResult(500);
        }
    }
}