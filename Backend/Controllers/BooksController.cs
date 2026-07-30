using BookTracker.Api.Auth;
using BookTracker.Api.DTOs;
using BookTracker.Api.Enums;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var userId = User.GetUserId();
        var result = await _bookService.GetUserBooks(userId);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var userId = User.GetUserId();
        var result = await _bookService.GetUserBook(userId, id);

        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateUserBookDTO dto)
    {
        var userId = User.GetUserId();
        var result = await _bookService.UpdateUserBook(userId, id, dto);

        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> FindBook([FromQuery] string query)
    {
        var userId = User.GetUserId();
        var result = await _bookService.FindBook(query, userId);

        if (result != null && result.Count > 0)
        {
            return Ok(result);
        }

        return NotFound("Couldn't find book.");
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBookToUser([FromQuery] int bookId)
    {
        var userId = User.GetUserId();
        var result = await _bookService.AddBookToUser(bookId, userId);

        if (result == null) return StatusCode(500);

        return result.AddBookStatus switch
        {
            AddBookStatus.BookNotFound or AddBookStatus.UserNotFound => NotFound(result),
            AddBookStatus.AlreadyInLibrary => Conflict(result),
            AddBookStatus.Success => Created("api/books/add", result),
            _ => StatusCode(500, "Unknown status."),
        };
    }
}