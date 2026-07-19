using BookTracker.Api.DTOs;

namespace BookTracker.Api.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string username, string password);
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
}