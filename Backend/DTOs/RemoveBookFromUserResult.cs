namespace BookTracker.Api.DTOs;

public class RemoveBookFromUserResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static RemoveBookFromUserResult Ok() => new()
    {
        Success = true,
        Message = "Book removed from list"
    };

    public static RemoveBookFromUserResult NotFoundResult() => new()
    {
        Success = false,
        Message = "Book not found in your library"
    };
}
