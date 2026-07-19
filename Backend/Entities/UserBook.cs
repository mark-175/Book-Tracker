using BookTracker.Api.Enums;

namespace BookTracker.Api.Entities;

public class UserBook
{
    public int Id { get; set; }
    public BookStatus Status { get; set; } = BookStatus.ToRead;

    public double Rating { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;


    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}