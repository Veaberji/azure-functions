using EDU.Models;

namespace EDU.Services;

public interface ISentimentService
{
    Task<TweetSummary> AnalyzeTweetsAsync(List<AnalyzedTweet> tweets);
}
