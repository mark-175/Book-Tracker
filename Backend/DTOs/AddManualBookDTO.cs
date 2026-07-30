using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.DTOs;

public class AddManualBookDTO
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Authors { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Subtitle { get; set; }

    [MaxLength(20)]
    public string? Isbn10 { get; set; }

    [MaxLength(20)]
    public string? Isbn13 { get; set; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; set; }

    [MaxLength(10000)]
    public string? Description { get; set; }
}
