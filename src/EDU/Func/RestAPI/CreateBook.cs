using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Models;
using Microsoft.Azure.Cosmos;

namespace EDU.Func.RestAPI;

public class CreateBook(ILogger<CreateBook> logger, CosmosClient cosmosClient)
{
    [Function("CreateBook")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var requestBody = await req.ReadFromJsonAsync<Book>();
        if (requestBody is null || string.IsNullOrEmpty(requestBody.Title))
        {
            return new BadRequestObjectResult("Parameter missing: Title of the book");
        }

        var itemBody = new Book
        {
            Id = Guid.NewGuid().ToString(),
            Author = requestBody.Author,
            Title = requestBody.Title,
            DatePublished = requestBody.DatePublished
        };
        try
        {
            var database = cosmosClient.GetDatabase("afccosmosdatabase");
            var container = database.GetContainer("afccosmoscollection");
            await container.CreateItemAsync(itemBody, new PartitionKey(itemBody.Author));

            return new OkObjectResult("Item added successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating book");
            return new StatusCodeResult(500);
        }
    }
}