using BookTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Tests.TestHelpers;

public static class InMemoryDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
