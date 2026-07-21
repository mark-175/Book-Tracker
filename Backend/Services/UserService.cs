using BookTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Services;

public class UserService : IUserService
{
    private AppDbContext _dbContext;

    public UserService(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }
    public async Task<List<string>> GetPreferredLanguages(Guid userId)
    {
        var preferredLanguages = await _dbContext.Users
        .Where(u => u.Id == userId)
        .Select(u => u.PreferredLanguages)
        .FirstOrDefaultAsync();

        if (preferredLanguages is null || preferredLanguages == "") return ["en"];

        return [.. preferredLanguages.Split(",")];
    }
}