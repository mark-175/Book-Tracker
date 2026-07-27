using BookTracker.Api.Enums;

namespace BookTracker.Api.DTOs;

public class UpdateUserBookDTO
{
    public BookStatus? Status { get; set; }
    public double? Rating { get; set; }
    public int? PagesRead { get; set; }
    public string? Notes { get; set; }
}
