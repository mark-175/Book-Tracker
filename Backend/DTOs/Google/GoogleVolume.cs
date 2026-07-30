using System.Text.Json.Serialization;

namespace BookTracker.Api.DTOs.Google;

public class GoogleVolume
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("volumeInfo")]
    public GoogleVolumeInfo VolumeInfo { get; set; } = new();
}