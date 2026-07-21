using System.Text.Json.Serialization;
using BookTracker.Api.Entities;

namespace BookTracker.Api.DTOs.Google;

public class GoogleVolume
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("volumeInfo")]
    public GoogleVolumeInfo VolumeInfo { get; set; } = new();

    public static Book ToBook(GoogleVolume volume)
    {
        return new Book
        {
            GoogleBooksId = volume.Id,
            Title = volume.VolumeInfo.Title,
            Subtitle = volume.VolumeInfo.Subtitle ?? string.Empty,
            Authors = string.Join(", ", volume.VolumeInfo.Authors ?? new List<string>()),
            Language = volume.VolumeInfo.Language ?? string.Empty,
            Description = volume.VolumeInfo.Description,
            CoverUrl = volume.VolumeInfo.ImageLinks?.Thumbnail,
            Isbn10 = volume.VolumeInfo.IndustryIdentifiers?
                .FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier,
            Isbn13 = volume.VolumeInfo.IndustryIdentifiers?
                .FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier,
            PageCount = volume.VolumeInfo.PageCount,
            CachedAt = DateTime.UtcNow
        };
    }
}