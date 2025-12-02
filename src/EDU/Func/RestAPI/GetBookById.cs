using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Models;
using Microsoft.Azure.Cosmos;

namespace EDU.Func.RestAPI;

public class GetBookById(ILogger<GetBookById> logger, CosmosClient cosmosClient)
{
    [Function("GetBookById")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        if (string.IsNullOrEmpty(id))
        {
            return new BadRequestObjectResult("Id parameter is required");
        }

        try
        {
            var database = cosmosClient.GetDatabase("afccosmosdatabase");
            var container = database.GetContainer("afccosmoscollection");
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", id);
            var iterator = container.GetItemQueryIterator<Book>(query);
            var results = new List<Book>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            var book = results.FirstOrDefault();
            if (book is not null)
            {
                return new OkObjectResult(book);
            }
            else
            {
                return new NotFoundObjectResult("Book not found");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving book");
            return new StatusCodeResult(500);
        }
    }
}