using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EDU.Models;
using EDU.Services;

namespace EDU.Func;

public class TwitterSentiment(ILogger<TwitterSentiment> logger, ITwitterService twitterService, ISentimentService sentimentService)
{
    [Function("TwitterSentiment")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string? hashtag = req.Query["hashtag"];
        if (string.IsNullOrEmpty(hashtag))
        {
            return new BadRequestObjectResult(new { message = "Provide hashtag as parameter" });
        }

        try
        {
            var tweets = await twitterService.SearchTweetsAsync(hashtag);
            if (tweets.Count == 0)
            {
                return new OkObjectResult(new TweetSummary());
            }

            var summary = await sentimentService.AnalyzeTweetsAsync(tweets);

            return new OkObjectResult(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing request");
            return new ObjectResult(new { message = "Internal Server Error" }) { StatusCode = 500 };
        }
    }
}
