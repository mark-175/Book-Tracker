namespace BookTracker.Api.DTOs;

public class BookSearchResult
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public int? PageCount { get; set; }

}