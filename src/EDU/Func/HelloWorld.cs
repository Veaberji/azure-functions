using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EDU.Func;

public class HelloWorld(ILogger<HelloWorld> logger)
{
    [Function("HelloWorld")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? name = req.Query["name"];
        string message = string.IsNullOrEmpty(name) ? "Welcome to Azure Functions!" : $"Hello, {name}! Welcome to Azure Functions!";

        return new OkObjectResult(message);
    }
}