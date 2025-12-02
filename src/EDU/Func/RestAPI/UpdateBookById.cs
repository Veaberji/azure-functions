using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Models;
using Microsoft.Azure.Cosmos;

namespace EDU.Func.RestAPI;

public class UpdateBookById(ILogger<UpdateBookById> logger, CosmosClient cosmosClient)
{
    [Function("UpdateBookById")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequest req)
    {
        logger.LogInformation("UpdateBookById Function Triggered");

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
            if (book is null)
            {
                return new NotFoundObjectResult("Book with given Id not found");
            }

            var requestBody = await req.ReadFromJsonAsync<Book>();

            var updatedBook = new Book
            {
                Id = book.Id,
                Author = requestBody?.Author ?? book.Author,
                Title = requestBody?.Title ?? book.Title,
                DatePublished = requestBody?.DatePublished ?? book.DatePublished
            };

            await container.UpsertItemAsync(updatedBook, new PartitionKey(updatedBook.Author));

            return new OkObjectResult(updatedBook);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating book");
            return new StatusCodeResult(500);
        }
    }
}