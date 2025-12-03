namespace EDU.Models;

public class TweetSummary
{
    public List<AnalyzedTweet> PositiveTweets { get; set; } = new();

    public List<AnalyzedTweet> NegativeTweets { get; set; } = new();
}
