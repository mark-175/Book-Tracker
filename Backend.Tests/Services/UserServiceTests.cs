using BookTracker.Api.Entities;
using BookTracker.Api.Services;
using BookTracker.Api.Tests.TestHelpers;

namespace BookTracker.Api.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetPreferredLanguages_UserWithMultipleLanguages_ReturnsSplitList()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PreferredLanguages = "en,fr" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetPreferredLanguages(user.Id);

        Assert.Equal(new List<string> { "en", "fr" }, result);
    }

    [Fact]
    public async Task GetPreferredLanguages_UserWithSingleLanguage_ReturnsSingleItemList()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PreferredLanguages = "en" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetPreferredLanguages(user.Id);

        Assert.Equal(new List<string> { "en" }, result);
    }

    [Fact]
    public async Task GetPreferredLanguages_UserWithEmptyString_ReturnsListWithEmptyString()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PreferredLanguages = "" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetPreferredLanguages(user.Id);

        Assert.Equal(new List<string> { "" }, result);
    }

    [Fact]
    public async Task GetPreferredLanguages_UserNotFound_ReturnsListWithEmptyString()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = new UserService(db);

        var result = await service.GetPreferredLanguages(Guid.NewGuid());

        Assert.Equal(new List<string> { "" }, result);
    }

    [Fact]
    public async Task GetUser_UserExists_ReturnsUser()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "alice" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetUser(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task GetUser_UserNotFound_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = new UserService(db);

        var result = await service.GetUser(Guid.NewGuid());

        Assert.Null(result);
    }
}
