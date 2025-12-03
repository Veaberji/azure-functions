using System.Text.Json.Serialization;

namespace EDU.Models;

public class TwitterSearchResponse
{
    [JsonPropertyName("statuses")]
    public List<TweetStatus> Statuses { get; set; } = new();
}
