namespace BookTracker.Api.Services;

public interface IUserService
{
    public Task<List<string>> GetPreferredLanguages(Guid userId);   
}