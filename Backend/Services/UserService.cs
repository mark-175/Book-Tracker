using BookTracker.Api.Data;
using BookTracker.Api.Entities;
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

        if (preferredLanguages is null || preferredLanguages == "") return [""];

        return [.. preferredLanguages.Split(",")];
    }

    public async Task<User?> GetUser(Guid userId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}