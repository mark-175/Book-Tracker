using BookTracker.Api.Entities;

namespace BookTracker.Api.Services;

public interface IUserService
{
    public Task<List<string>> GetPreferredLanguages(Guid userId);
    public Task<User?> GetUser(Guid userId);
}