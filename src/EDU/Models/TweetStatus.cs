using System.Text.Json.Serialization;

namespace EDU.Models;

public class TweetStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;
}
