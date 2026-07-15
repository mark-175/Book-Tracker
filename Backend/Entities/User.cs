using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public string GoogleSub { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}