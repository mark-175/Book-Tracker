using BookTracker.Api.Data;
using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;
using BookTracker.Api.Enums;
using BookTracker.Api.Services.Db;
using BookTracker.Api.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookTracker.Api.Tests.Services.Db;

public class DbBookServiceTests
{
    private static DbBookService CreateService(AppDbContext dbContext)
    {
        return new DbBookService(dbContext, Mock.Of<ILogger<DbBookService>>());
    }

    [Fact]
    public async Task FindOrCreateManualBook_NoIsbnProvided_CreatesNewManualBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = CreateService(dbContext);
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert" };

        var book = await service.FindOrCreateManualBook(dto);

        Assert.NotEqual(0, book.Id);
        Assert.Equal(BookSource.Manual, book.Source);
        Assert.Null(book.GoogleBooksId);
        Assert.Single(dbContext.Books);
    }

    [Fact]
    public async Task FindOrCreateManualBook_MatchingIsbn13_ReusesExistingBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var existing = new Book { Title = "Dune", Authors = "Frank Herbert", Isbn13 = "9780441013593" };
        dbContext.Books.Add(existing);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var dto = new AddManualBookDTO { Title = "Dune (dup)", Authors = "F. Herbert", Isbn13 = "9780441013593" };

        var book = await service.FindOrCreateManualBook(dto);

        Assert.Equal(existing.Id, book.Id);
        Assert.Single(dbContext.Books);
    }

    [Fact]
    public async Task FindOrCreateManualBook_MatchingIsbn10_ReusesExistingBookWhenNoIsbn13Match()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var existing = new Book { Title = "Dune", Authors = "Frank Herbert", Isbn10 = "0441013597" };
        dbContext.Books.Add(existing);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var dto = new AddManualBookDTO { Title = "Dune (dup)", Authors = "F. Herbert", Isbn10 = "0441013597" };

        var book = await service.FindOrCreateManualBook(dto);

        Assert.Equal(existing.Id, book.Id);
        Assert.Single(dbContext.Books);
    }

    [Fact]
    public async Task FindOrCreateManualBook_IsbnProvidedButNoMatch_CreatesNewBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = CreateService(dbContext);
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert", Isbn13 = "9780441013593" };

        var book = await service.FindOrCreateManualBook(dto);

        Assert.Equal("9780441013593", book.Isbn13);
        Assert.Single(dbContext.Books);
    }
}
