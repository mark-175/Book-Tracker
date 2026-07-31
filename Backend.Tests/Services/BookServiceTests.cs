using BookTracker.Api.DTOs;
using BookTracker.Api.DTOs.Google;
using BookTracker.Api.Entities;
using BookTracker.Api.Services;
using BookTracker.Api.Services.Db;
using BookTracker.Api.Services.Google;
using Moq;

namespace BookTracker.Api.Tests.Services;

public class BookServiceTests
{
    [Fact]
    public async Task AddManualBook_DelegatesToDbBookServiceAndMapsResult()
    {
        var book = new Book { Id = 42, Title = "Dune", Authors = "Frank Herbert" };
        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock
            .Setup(s => s.FindOrCreateManualBook(It.IsAny<AddManualBookDTO>()))
            .ReturnsAsync(book);

        var service = new BookService(
            dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert" };

        var result = await service.AddManualBook(dto);

        Assert.Equal(42, result.Id);
        Assert.Equal("Dune", result.Title);
        dbBookServiceMock.Verify(s => s.FindOrCreateManualBook(dto), Times.Once);
    }

    [Fact]
    public async Task FindBook_DbResultNonEmpty_ReturnsDbResultAndSkipsGoogle()
    {
        var userId = Guid.NewGuid();
        var dbResult = new List<BookSearchResult>
        {
            new() { Id = 1, Title = "Dune" }
        };
        var preferredLanguages = new List<string> { "en" };

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetPreferredLanguages(userId)).ReturnsAsync(preferredLanguages);

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock
            .Setup(s => s.FindBookInDb("dune", preferredLanguages))
            .ReturnsAsync(dbResult);

        var googleBookServiceMock = new Mock<IGoogleBookService>();

        var service = new BookService(dbBookServiceMock.Object, googleBookServiceMock.Object, userServiceMock.Object);

        var result = await service.FindBook("dune", userId);

        Assert.Same(dbResult, result);
        googleBookServiceMock.Verify(
            s => s.FindBookInGoogle(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task FindBook_DbResultEmpty_FallsBackToGoogleAndPersistsMappedBooks()
    {
        var userId = Guid.NewGuid();
        var preferredLanguages = new List<string> { "en" };

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetPreferredLanguages(userId)).ReturnsAsync(preferredLanguages);

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock
            .Setup(s => s.FindBookInDb("dune", preferredLanguages))
            .ReturnsAsync([]);

        var volume = new GoogleVolume
        {
            Id = "google-id-1",
            VolumeInfo = new GoogleVolumeInfo { Title = "Dune (Mapped)" }
        };
        var googleResponse = new GoogleBooksSearchResponse
        {
            TotalItems = 1,
            Items = [volume]
        };

        var googleBookServiceMock = new Mock<IGoogleBookService>();
        googleBookServiceMock
            .Setup(s => s.FindBookInGoogle("dune", preferredLanguages))
            .ReturnsAsync(googleResponse);

        var persistedBook = new Book { Id = 99, Title = "Dune (Persisted)", Authors = "Frank Herbert" };
        dbBookServiceMock
            .Setup(s => s.AddBookToDb(It.Is<Book>(b => b.GoogleBooksId == "google-id-1" && b.Title == "Dune (Mapped)")))
            .ReturnsAsync(persistedBook);

        var service = new BookService(dbBookServiceMock.Object, googleBookServiceMock.Object, userServiceMock.Object);

        var result = await service.FindBook("dune", userId);

        var single = Assert.Single(result);
        Assert.Equal(99, single.Id);
        Assert.Equal("Dune (Persisted)", single.Title);
        dbBookServiceMock.Verify(s => s.AddBookToDb(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task FindBook_GoogleReturnsNull_ReturnsEmptyListWithoutThrowing()
    {
        var userId = Guid.NewGuid();
        var preferredLanguages = new List<string> { "en" };

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetPreferredLanguages(userId)).ReturnsAsync(preferredLanguages);

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock
            .Setup(s => s.FindBookInDb("dune", preferredLanguages))
            .ReturnsAsync([]);

        var googleBookServiceMock = new Mock<IGoogleBookService>();
        googleBookServiceMock
            .Setup(s => s.FindBookInGoogle("dune", preferredLanguages))
            .ReturnsAsync((GoogleBooksSearchResponse?)null);

        var service = new BookService(dbBookServiceMock.Object, googleBookServiceMock.Object, userServiceMock.Object);

        var result = await service.FindBook("dune", userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserBooks_ReturnsDbBookServiceResult()
    {
        var userId = Guid.NewGuid();
        var expected = new List<UserBookDTO>
        {
            new() { BookId = 1, Title = "Dune" }
        };

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.GetUserBooks(userId)).ReturnsAsync(expected);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.GetUserBooks(userId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetUserBook_BookExists_ReturnsDbBookServiceResult()
    {
        var userId = Guid.NewGuid();
        var expected = new UserBookDTO { BookId = 1, Title = "Dune" };

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.GetUserBook(userId, 1)).ReturnsAsync(expected);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.GetUserBook(userId, 1);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetUserBook_BookDoesNotExist_ReturnsNull()
    {
        var userId = Guid.NewGuid();

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.GetUserBook(userId, 1)).ReturnsAsync((UserBookDTO?)null);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.GetUserBook(userId, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserBook_ReturnsDbBookServiceResult()
    {
        var userId = Guid.NewGuid();
        var dto = new UpdateUserBookDTO { Rating = 4 };
        var expected = new UserBookDTO { BookId = 1, Title = "Dune", Rating = 4 };

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.UpdateUserBook(userId, 1, dto)).ReturnsAsync(expected);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.UpdateUserBook(userId, 1, dto);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task UpdateUserBook_BookDoesNotExist_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var dto = new UpdateUserBookDTO { Rating = 4 };

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.UpdateUserBook(userId, 1, dto)).ReturnsAsync((UserBookDTO?)null);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.UpdateUserBook(userId, 1, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveBookFromUser_ReturnsDbBookServiceResult()
    {
        var userId = Guid.NewGuid();
        var expected = RemoveBookFromUserResult.Ok();

        var dbBookServiceMock = new Mock<IDbBookService>();
        dbBookServiceMock.Setup(s => s.RemoveBookFromUser(userId, 1)).ReturnsAsync(expected);

        var service = new BookService(dbBookServiceMock.Object, Mock.Of<IGoogleBookService>(), Mock.Of<IUserService>());

        var result = await service.RemoveBookFromUser(userId, 1);

        Assert.Same(expected, result);
    }
}
