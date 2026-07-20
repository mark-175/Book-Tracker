using BookTracker.Api.Auth;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> FindBook([FromQuery] string query)
    {
        var userId = User.GetUserId();
        var result = _bookService.FindBook(query, userId);

        if (result != null)
        {
            return Ok(result);
        }

        return NotFound("Couldn't find book.");
    }
}