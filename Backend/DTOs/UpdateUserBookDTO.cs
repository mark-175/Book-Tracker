using System.ComponentModel.DataAnnotations;
using BookTracker.Api.Enums;

namespace BookTracker.Api.DTOs;

public class UpdateUserBookDTO
{
    public BookStatus? Status { get; set; }

    [Range(0, 5)]
    public double? Rating { get; set; }

    [Range(0, int.MaxValue)]
    public int? PagesRead { get; set; }

    [MaxLength(10000)]
    public string? Notes { get; set; }
}
