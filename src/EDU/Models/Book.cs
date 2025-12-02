using System.Text.Json.Serialization;

namespace EDU.Models;

public class Book
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("date_published")]
    public string? DatePublished { get; set; }
}