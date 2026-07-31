using System.Security.Claims;
using BookTracker.Api.Controllers;
using BookTracker.Api.DTOs;
using BookTracker.Api.Enums;
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

    [Fact]
    public async Task AddBookToUser_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var result = AddBookToUserResult.UserNotFoundResult(1);
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddBookToUser(1, userId)).ReturnsAsync(result);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.AddBookToUser(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(result, notFound.Value);
    }

    [Fact]
    public async Task AddBookToUser_UnmappedStatus_ReturnsStatusCode500()
    {
        var userId = Guid.NewGuid();
        var result = AddBookToUserResult.Success(userId, 1, new BookDTO { Title = "Dune" });
        result.AddBookStatus = (AddBookStatus)999;
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.AddBookToUser(1, userId)).ReturnsAsync(result);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.AddBookToUser(1);

        var statusResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_ReturnsOkWithUserBooks()
    {
        var userId = Guid.NewGuid();
        var expected = new List<UserBookDTO> { new() { BookId = 1, Title = "Dune" } };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.GetUserBooks(userId)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.GetBooks();

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetBook_Found_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var expected = new UserBookDTO { BookId = 1, Title = "Dune" };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.GetUserBook(userId, 1)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.GetBook(1);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetBook_NotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.GetUserBook(userId, 1)).ReturnsAsync((UserBookDTO?)null);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.GetBook(1);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task UpdateBook_Found_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var dto = new UpdateUserBookDTO { Rating = 4 };
        var expected = new UserBookDTO { BookId = 1, Title = "Dune", Rating = 4 };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.UpdateUserBook(userId, 1, dto)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.UpdateBook(1, dto);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task UpdateBook_NotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var dto = new UpdateUserBookDTO { Rating = 4 };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.UpdateUserBook(userId, 1, dto)).ReturnsAsync((UserBookDTO?)null);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.UpdateBook(1, dto);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task FindBook_ResultsFound_ReturnsOkWithResults()
    {
        var userId = Guid.NewGuid();
        var expected = new List<BookSearchResult> { new() { Id = 1, Title = "Dune" } };
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.FindBook("dune", userId)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.FindBook("dune");

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task FindBook_EmptyResults_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.FindBook("dune", userId)).ReturnsAsync([]);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.FindBook("dune");

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Couldn't find book.", notFound.Value);
    }

    [Fact]
    public async Task FindBook_NullResults_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.FindBook("dune", userId)).ReturnsAsync((List<BookSearchResult>)null!);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.FindBook("dune");

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal("Couldn't find book.", notFound.Value);
    }

    [Fact]
    public async Task DeleteBook_Success_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var expected = RemoveBookFromUserResult.Ok();
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.RemoveBookFromUser(userId, 1)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.DeleteBook(1);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task DeleteBook_NotFound_ReturnsNotFoundWithResult()
    {
        var userId = Guid.NewGuid();
        var expected = RemoveBookFromUserResult.NotFoundResult();
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock.Setup(s => s.RemoveBookFromUser(userId, 1)).ReturnsAsync(expected);
        var controller = CreateController(bookServiceMock.Object, userId);

        var response = await controller.DeleteBook(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Same(expected, notFound.Value);
    }
}
