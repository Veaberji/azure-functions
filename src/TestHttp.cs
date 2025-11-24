using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SDTP.Func;

public class TestHttp(ILogger<TestHttp> logger)
{
    private readonly ILogger<TestHttp> _logger = logger;

    [Function("TestHttp")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}