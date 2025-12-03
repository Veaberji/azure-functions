using EDU.Models;

namespace EDU.Services;

public interface ITwitterService
{
    Task<List<AnalyzedTweet>> SearchTweetsAsync(string hashtag);
}
