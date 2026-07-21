using System.Text.Json.Serialization;

namespace BookTracker.Api.DTOs.Google;

public class GoogleImageLinks
{
    [JsonPropertyName("smallThumbnail")]
    public string? SmallThumbnail { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}