using Azure.AI.TextAnalytics;
using EDU.Models;
using Microsoft.Extensions.Logging;

namespace EDU.Services;

public class SentimentService(TextAnalyticsClient textAnalyticsClient, ILogger<SentimentService> logger) : ISentimentService
{
    public async Task<TweetSummary> AnalyzeTweetsAsync(List<AnalyzedTweet> tweets)
    {
        var summary = new TweetSummary();
        var documents = tweets.Select(t => new TextDocumentInput(t.Id, t.Text) { Language = "en" }).ToList();

        if (!documents.Any())
        {
            return summary;
        }

        int batchSize = 10;
        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var batch = documents.Skip(i).Take(batchSize).ToList();
            var response = await textAnalyticsClient.AnalyzeSentimentBatchAsync(batch);
            var results = response.Value;

            foreach (AnalyzeSentimentResult result in results)
            {
                if (result.HasError)
                {
                    logger.LogError($"Document error: {result.Error.Message}");
                    continue;
                }

                var tweet = tweets.FirstOrDefault(t => t.Id == result.Id);
                if (tweet is null)
                {
                    continue;
                }

                if (result.DocumentSentiment.Sentiment == TextSentiment.Negative)
                {
                    summary.NegativeTweets.Add(tweet);
                }
                else
                {
                    summary.PositiveTweets.Add(tweet);
                }
            }
        }

        return summary;
    }
}
