using System.Text.Json.Serialization;

namespace BookTracker.Api.DTOs.Google;

public class GoogleVolumeInfo
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("industryIdentifiers")]
    public List<GoogleIndustryIdentifier>? IndustryIdentifiers { get; set; }

    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("imageLinks")]
    public GoogleImageLinks? ImageLinks { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}