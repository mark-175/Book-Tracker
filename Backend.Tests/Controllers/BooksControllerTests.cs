using System.Security.Claims;
using BookTracker.Api.Controllers;
using BookTracker.Api.DTOs;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookTracker.Api.Tests.Controllers;

public class BooksControllerTests
{
    private static BooksController CreateController(IBookService bookService, Guid? userId = null)
    {
        var controller = new BooksController(bookService);

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
    public async Task AddManualBook_ValidDto_ReturnsCreatedWithResult()
    {
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert" };
        var expected = new BookSearchResult { Id = 1, Title = "Dune", Authors = "Frank Herbert" };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddManualBook(dto)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object);

        var response = await controller.AddManualBook(dto);

        var created = Assert.IsType<CreatedResult>(response);
        Assert.Equal(expected, created.Value);
    }

    [Fact]
    public async Task AddBookToUser_Success_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var result = AddBookToUserResult.Success(userId, 1, new BookDTO { Title = "Dune" });
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddBookToUser(1, userId)).ReturnsAsync(result);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.AddBookToUser(1);

        Assert.IsType<CreatedResult>(response);
    }

    [Fact]
    public async Task AddBookToUser_AlreadyInLibrary_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var result = AddBookToUserResult.AlreadyInLibraryResult(userId, 1);
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddBookToUser(1, userId)).ReturnsAsync(result);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.AddBookToUser(1);

        Assert.IsType<ConflictObjectResult>(response);
    }

    [Fact]
    public async Task AddBookToUser_BookNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var result = AddBookToUserResult.BookNotFoundResult(userId, 1);
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddBookToUser(1, userId)).ReturnsAsync(result);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.AddBookToUser(1);

        Assert.IsType<NotFoundObjectResult>(response);
    }
}
