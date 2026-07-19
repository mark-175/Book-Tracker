namespace BookTracker.Api.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static AuthResult Ok() => new() { Success = true };
    public static AuthResult Fail(string error) => new() { Success = false, Error = error };
}