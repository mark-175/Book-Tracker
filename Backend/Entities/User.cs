using System.ComponentModel.DataAnnotations;

namespace BookTracker.Api.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}