using System.Text.Json.Serialization;

namespace BookTracker.Api.DTOs.Google;

public class GoogleBooksSearchResponse
{
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("items")]
    public List<GoogleVolume> Items { get; set; } = new();
}