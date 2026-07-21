using System.Text.Json.Serialization;

namespace BookTracker.Api.DTOs.Google;
    
public class GoogleIndustryIdentifier
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;
}