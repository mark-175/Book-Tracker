using BookTracker.Api.DTOs;
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
}
