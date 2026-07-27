using BookTracker.Api.Data;
using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;

namespace BookTracker.Api.Services.Db;

public class DbBookService : IDbBookService
{
    private AppDbContext _dbContext;
    private readonly ILogger<DbBookService> _logger;

    public DbBookService(AppDbContext appDbContext, ILogger<DbBookService> logger)
    {
        _dbContext = appDbContext;
        _logger = logger;
    }

    public async Task<List<BookSearchResult>> FindBookInDb(string query, Guid userId, List<string> preferredLanguages)
    {
        var books = await _dbContext.Books
            .Where(b => b.Title.ToUpper().Contains(query.ToUpper()) &&
                preferredLanguages.Contains(b.Language))
            .Select(b => BookMapper.ToSearchResult(b))
            .ToListAsync();

        return books;
    }

    public async Task AddBookToDb(Book book)
    {
        var existing = await _dbContext.Books
            .FirstOrDefaultAsync(b => b.GoogleBooksId == book.GoogleBooksId);

        if (existing is not null)
            return;

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AddBookToUserResult> AddBookToUser(int bookId, Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            _logger.LogWarning("AddBookToUser called for nonexistent user {UserId}", userId);
            return AddBookToUserResult.UserNotFoundResult(bookId);
        }

        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null)
        {
            _logger.LogWarning("AddBookToUser called for nonexistent book {BookId}", bookId);
            return AddBookToUserResult.BookNotFoundResult(userId, bookId);
        }

        var userBook = new UserBook
        {
            UserId = userId,
            BookId = bookId,
        };
        _dbContext.UserBooks.Add(userBook);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogWarning(e, "Failed to add book {BookId} to user {UserId}'s library, " +
                "likely already in their active library", bookId, userId);
            return AddBookToUserResult.UnexpectedError(bookId);
        }

        return AddBookToUserResult.Success(userId, bookId, BookMapper.ToBookDTO(book));
    }

    public async Task<List<UserBookDTO>> GetUserBooks(Guid userId)
    {
        var userBooks = await _dbContext.UserBooks
            .Include(ub => ub.Book)
            .Where(ub => ub.UserId == userId)
            .ToListAsync();

        return userBooks.Select(BookMapper.ToUserBookDTO).ToList();
    }

    public async Task<UserBookDTO?> GetUserBook(Guid userId, int bookId)
    {
        var userBook = await _dbContext.UserBooks
            .Include(ub => ub.Book)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

        return userBook is null ? null : BookMapper.ToUserBookDTO(userBook);
    }
}