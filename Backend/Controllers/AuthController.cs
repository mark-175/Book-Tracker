using BookTracker.Api.Auth;
using BookTracker.Api.DTOs;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Login dto)
    {
        var result = await _authService.RegisterAsync(dto.Username, dto.Password);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);
        return result.Success ? Ok() : Unauthorized(result.Error);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var user = await _userService.GetUser(userId);

        if (user is null) return NotFound();

        return Ok(new UserDTO { Id = user.Id, Username = user.Username });
    }
}