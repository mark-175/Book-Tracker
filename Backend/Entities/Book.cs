using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Entities;

public class Book
{
    public int Id { get; set; }

    [Required]
    public string GoogleBooksId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Subtitle {get;set;} = string.Empty;

    public string Authors { get; set; } = string.Empty;

    public string? Isbn10 { get; set; }

    public string? Isbn13 { get; set; }

    public string? CoverUrl { get; set; }

    public string? Description { get; set; }

    public int? PageCount { get; set; }

    public DateTime CachedAt { get; set; } = DateTime.UtcNow;


    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}