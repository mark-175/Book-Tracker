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

    [Fact]
    public async Task AddBookToUser_UnknownUser_ReturnsUserNotFound()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var book = new Book { Title = "Dune", Authors = "Frank Herbert" };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.AddBookToUser(book.Id, Guid.NewGuid());

        Assert.Equal(AddBookStatus.UserNotFound, result.AddBookStatus);
    }

    [Fact]
    public async Task AddBookToUser_UnknownBook_ReturnsBookNotFound()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "reader", PasswordHash = "hash" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.AddBookToUser(999, user.Id);

        Assert.Equal(AddBookStatus.BookNotFound, result.AddBookStatus);
    }

    [Fact]
    public async Task AddBookToUser_NewBookForUser_ReturnsSuccessAndCreatesUserBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "reader", PasswordHash = "hash" };
        var book = new Book { Title = "Dune", Authors = "Frank Herbert" };
        dbContext.Users.Add(user);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.AddBookToUser(book.Id, user.Id);

        Assert.Equal(AddBookStatus.Success, result.AddBookStatus);
        Assert.NotNull(result.Book);
        Assert.Single(dbContext.UserBooks);
    }

    [Fact]
    public async Task AddBookToUser_AlreadyInActiveLibrary_ReturnsAlreadyInLibrary()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "reader", PasswordHash = "hash" };
        var book = new Book { Title = "Dune", Authors = "Frank Herbert" };
        dbContext.Users.Add(user);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        dbContext.UserBooks.Add(new UserBook { UserId = user.Id, BookId = book.Id });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.AddBookToUser(book.Id, user.Id);

        Assert.Equal(AddBookStatus.AlreadyInLibrary, result.AddBookStatus);
        Assert.Single(dbContext.UserBooks);
    }

    [Fact]
    public async Task AddBookToUser_PreviouslySoftDeleted_AllowsReAdding()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var user = new User { Id = Guid.NewGuid(), Username = "reader", PasswordHash = "hash" };
        var book = new Book { Title = "Dune", Authors = "Frank Herbert" };
        dbContext.Users.Add(user);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        dbContext.UserBooks.Add(new UserBook
        {
            UserId = user.Id,
            BookId = book.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.AddBookToUser(book.Id, user.Id);

        Assert.Equal(AddBookStatus.Success, result.AddBookStatus);
    }

    [Fact]
    public async Task FindBookInDb_TitleContainsQueryCaseInsensitive_ReturnsMatchingBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var book = new Book { Title = "The Great Gatsby", Authors = "F. Scott Fitzgerald", Language = "en" };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var results = await service.FindBookInDb("great", new List<string> { "en" });

        Assert.Single(results);
        Assert.Equal(book.Id, results[0].Id);
    }

    [Fact]
    public async Task FindBookInDb_QueryInDifferentCase_StillMatches()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var book = new Book { Title = "The Great Gatsby", Authors = "F. Scott Fitzgerald", Language = "en" };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var results = await service.FindBookInDb("GREAT", new List<string> { "en" });

        Assert.Single(results);
        Assert.Equal(book.Id, results[0].Id);
    }

    [Fact]
    public async Task FindBookInDb_TitleMatchesButLanguageNotPreferred_ExcludesBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var book = new Book { Title = "The Great Gatsby", Authors = "F. Scott Fitzgerald", Language = "fr" };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var results = await service.FindBookInDb("great", new List<string> { "en" });

        Assert.Empty(results);
    }

    [Fact]
    public async Task FindBookInDb_NoTitleMatchesQuery_ReturnsEmptyList()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var book = new Book { Title = "The Great Gatsby", Authors = "F. Scott Fitzgerald", Language = "en" };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var results = await service.FindBookInDb("nonexistent", new List<string> { "en" });

        Assert.Empty(results);
    }

    [Fact]
    public async Task AddBookToDb_MatchingGoogleBooksId_ReturnsExistingBookWithoutCreatingDuplicate()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var existing = new Book { GoogleBooksId = "abc123", Title = "Dune", Authors = "Frank Herbert" };
        dbContext.Books.Add(existing);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);
        var incoming = new Book { GoogleBooksId = "abc123", Title = "Dune (incoming)", Authors = "Frank Herbert" };

        var result = await service.AddBookToDb(incoming);

        Assert.Equal(existing.Id, result.Id);
        Assert.Single(dbContext.Books);
    }

    [Fact]
    public async Task AddBookToDb_NoMatchingGoogleBooksId_PersistsNewBook()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = CreateService(dbContext);
        var incoming = new Book { GoogleBooksId = "new-id-456", Title = "Dune", Authors = "Frank Herbert" };

        var result = await service.AddBookToDb(incoming);

        Assert.NotEqual(0, result.Id);
        Assert.Single(dbContext.Books);
    }
}
