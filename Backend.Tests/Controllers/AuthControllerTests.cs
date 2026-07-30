using System.Security.Claims;
using BookTracker.Api.Auth;
using BookTracker.Api.Controllers;
using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookTracker.Api.Tests.Controllers;

public class AuthControllerTests
{
    private static AuthController CreateController(IAuthService authService, IUserService userService, Guid? userId = null)
    {
        var controller = new AuthController(authService, userService);

        var claims = new List<Claim>();
        if (userId is not null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    [Fact]
    public async Task Register_Success_ReturnsOk()
    {
        var dto = new Login { Username = "alice", Password = "Passw0rd!" };
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.RegisterAsync(dto.Username, dto.Password)).ReturnsAsync(AuthResult.Ok());
        var controller = CreateController(authServiceMock.Object, Mock.Of<IUserService>());

        var response = await controller.Register(dto);

        Assert.IsType<OkResult>(response);
    }

    [Fact]
    public async Task Register_Failure_ReturnsBadRequestWithError()
    {
        var dto = new Login { Username = "alice", Password = "bad" };
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.RegisterAsync(dto.Username, dto.Password)).ReturnsAsync(AuthResult.Fail("Invalid username."));
        var controller = CreateController(authServiceMock.Object, Mock.Of<IUserService>());

        var response = await controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal("Invalid username.", badRequest.Value);
    }

    [Fact]
    public async Task Login_Success_ReturnsOk()
    {
        var dto = new Login { Username = "alice", Password = "Passw0rd!" };
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.LoginAsync(dto.Username, dto.Password)).ReturnsAsync(AuthResult.Ok());
        var controller = CreateController(authServiceMock.Object, Mock.Of<IUserService>());

        var response = await controller.Login(dto);

        Assert.IsType<OkResult>(response);
    }

    [Fact]
    public async Task Login_Failure_ReturnsUnauthorizedWithError()
    {
        var dto = new Login { Username = "alice", Password = "wrong" };
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.LoginAsync(dto.Username, dto.Password)).ReturnsAsync(AuthResult.Fail("Invalid credentials."));
        var controller = CreateController(authServiceMock.Object, Mock.Of<IUserService>());

        var response = await controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response);
        Assert.Equal("Invalid credentials.", unauthorized.Value);
    }

    [Fact]
    public async Task Logout_Always_ReturnsOkAndCallsLogoutOnce()
    {
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.LogoutAsync()).Returns(Task.CompletedTask);
        var controller = CreateController(authServiceMock.Object, Mock.Of<IUserService>());

        var response = await controller.Logout();

        Assert.IsType<OkResult>(response);
        authServiceMock.Verify(s => s.LogoutAsync(), Times.Once);
    }

    [Fact]
    public async Task Me_UserFound_ReturnsOkWithUserDto()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "alice" };
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUser(userId)).ReturnsAsync(user);
        var controller = CreateController(Mock.Of<IAuthService>(), userServiceMock.Object, userId);

        var response = await controller.Me();

        var ok = Assert.IsType<OkObjectResult>(response);
        var dto = Assert.IsType<UserDTO>(ok.Value);
        Assert.Equal(userId, dto.Id);
        Assert.Equal("alice", dto.Username);
    }

    [Fact]
    public async Task Me_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUser(userId)).ReturnsAsync((User?)null);
        var controller = CreateController(Mock.Of<IAuthService>(), userServiceMock.Object, userId);

        var response = await controller.Me();

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Me_NoUserIdClaimOnPrincipal_ThrowsInvalidOperationException()
    {
        var controller = CreateController(Mock.Of<IAuthService>(), Mock.Of<IUserService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Me());
    }
}
