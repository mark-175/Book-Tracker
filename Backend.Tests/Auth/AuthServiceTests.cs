using System.Security.Claims;
using BookTracker.Api.Auth;
using BookTracker.Api.Data;
using BookTracker.Api.Entities;
using BookTracker.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookTracker.Api.Tests.Auth;

public class AuthServiceTests
{
    private static AuthService CreateService(AppDbContext db, IHttpContextAccessor accessor) =>
        new(db, accessor, Mock.Of<ILogger<AuthService>>());

    private static IHttpContextAccessor CreateNullHttpContextAccessor()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        return mock.Object;
    }

    private static (IHttpContextAccessor Accessor, Mock<IAuthenticationService> AuthServiceMock, DefaultHttpContext HttpContext) CreateWorkingHttpContextAccessor()
    {
        var authServiceMock = new Mock<IAuthenticationService>();
        var services = new ServiceCollection();
        services.AddSingleton(authServiceMock.Object);
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        return (accessorMock.Object, authServiceMock, httpContext);
    }

    [Fact]
    public async Task RegisterAsync_ValidUsernameAndPassword_PersistsUserWithHashedPassword()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = CreateService(db, CreateNullHttpContextAccessor());
        const string password = "Passw0rd!";

        var result = await service.RegisterAsync("alice", password);

        Assert.True(result.Success);
        var user = Assert.Single(db.Users);
        Assert.Equal("alice", user.Username);
        Assert.False(string.IsNullOrEmpty(user.PasswordHash));
        Assert.NotEqual(password, user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailure()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = "irrelevant" });
        await db.SaveChangesAsync();
        var service = CreateService(db, CreateNullHttpContextAccessor());

        var result = await service.RegisterAsync("alice", "Passw0rd!");

        Assert.False(result.Success);
        Assert.Equal("Invalid username.", result.Error);
        Assert.Single(db.Users);
    }

    [Theory]
    [InlineData("Sh0rt!")]
    [InlineData("abcdefg!")]
    [InlineData("abcdefg1")]
    public async Task RegisterAsync_PasswordFailsComplexityRules_ReturnsFailure(string invalidPassword)
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = CreateService(db, CreateNullHttpContextAccessor());

        var result = await service.RegisterAsync("alice", invalidPassword);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Error));
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task RegisterAsync_BoundaryValidPassword_ReturnsSuccess()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = CreateService(db, CreateNullHttpContextAccessor());

        var result = await service.RegisterAsync("alice", "abcdef1!");

        Assert.True(result.Success);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task LoginAsync_UnknownUsername_ReturnsInvalidCredentials()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = CreateService(db, CreateNullHttpContextAccessor());

        var result = await service.LoginAsync("ghost", "Passw0rd!");

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var registerService = CreateService(db, CreateNullHttpContextAccessor());
        await registerService.RegisterAsync("alice", "Passw0rd!");

        var result = await registerService.LoginAsync("alice", "WrongPass1!");

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_NullHttpContext_ThrowsInvalidOperationException()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var registerService = CreateService(db, CreateNullHttpContextAccessor());
        await registerService.RegisterAsync("alice", "Passw0rd!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => registerService.LoginAsync("alice", "Passw0rd!"));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_SignsInAndReturnsSuccess()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var registerService = CreateService(db, CreateNullHttpContextAccessor());
        await registerService.RegisterAsync("alice", "Passw0rd!");

        var (accessor, authServiceMock, httpContext) = CreateWorkingHttpContextAccessor();
        var service = CreateService(db, accessor);

        var result = await service.LoginAsync("alice", "Passw0rd!");

        Assert.True(result.Success);
        authServiceMock.Verify(
            s => s.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_NullHttpContext_ThrowsInvalidOperationException()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var service = CreateService(db, CreateNullHttpContextAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LogoutAsync());
    }

    [Fact]
    public async Task LogoutAsync_ValidHttpContext_SignsOut()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var (accessor, authServiceMock, httpContext) = CreateWorkingHttpContextAccessor();
        var service = CreateService(db, accessor);

        await service.LogoutAsync();

        authServiceMock.Verify(
            s => s.SignOutAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }
}
