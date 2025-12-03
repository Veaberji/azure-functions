using System.Net.Http.Headers;
using System.Text.Json;
using EDU.Models;
using Microsoft.Extensions.Configuration;

namespace EDU.Services;

public class TwitterService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ITwitterService
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient();
    private readonly string twitterToken = configuration["TWITTER_TOKEN"] ?? string.Empty;

    public async Task<List<AnalyzedTweet>> SearchTweetsAsync(string hashtag)
    {
        if (!hashtag.StartsWith("#"))
        {
            hashtag = "#" + hashtag;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitter.com/1.1/search/tweets.json?q={Uri.EscapeDataString(hashtag)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twitterToken);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<TwitterSearchResponse>(content);

        var parsedTweets = new List<AnalyzedTweet>();

        if (searchResponse?.Statuses is null)
        {
            return parsedTweets;
        }

        foreach (var tweet in searchResponse.Statuses)
        {
            parsedTweets.Add(new AnalyzedTweet
            {
                Id = tweet.Id,
                Text = tweet.Text
            });
        }

        return parsedTweets;
    }
}
