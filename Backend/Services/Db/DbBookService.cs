using BookTracker.Api.Data;
using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;
using BookTracker.Api.Enums;
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

    public async Task<List<BookSearchResult>> FindBookInDb(string query, List<string> preferredLanguages)
    {
        var books = await _dbContext.Books
            .Where(b => b.Title.ToUpper().Contains(query.ToUpper()) &&
                preferredLanguages.Contains(b.Language))
            .Select(b => BookMapper.ToSearchResult(b))
            .ToListAsync();

        return books;
    }

    public async Task<Book> AddBookToDb(Book book)
    {
        var existing = await _dbContext.Books
            .FirstOrDefaultAsync(b => b.GoogleBooksId == book.GoogleBooksId);

        if (existing is not null)
            return existing;

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
        return book;
    }

    public async Task<Book> FindOrCreateManualBook(AddManualBookDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Isbn13))
        {
            var existingByIsbn13 = await _dbContext.Books
                .FirstOrDefaultAsync(b => b.Isbn13 == dto.Isbn13);
            if (existingByIsbn13 is not null) return existingByIsbn13;
        }

        if (!string.IsNullOrWhiteSpace(dto.Isbn10))
        {
            var existingByIsbn10 = await _dbContext.Books
                .FirstOrDefaultAsync(b => b.Isbn10 == dto.Isbn10);
            if (existingByIsbn10 is not null) return existingByIsbn10;
        }

        var book = BookMapper.ToBook(dto);
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
        return book;
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

        var alreadyInLibrary = await _dbContext.UserBooks
            .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        if (alreadyInLibrary)
        {
            _logger.LogWarning("AddBookToUser called for book {BookId} already in user {UserId}'s library", bookId, userId);
            return AddBookToUserResult.AlreadyInLibraryResult(userId, bookId);
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

    public async Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto)
    {
        var userBook = await _dbContext.UserBooks
            .Include(ub => ub.Book)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

        if (userBook is null) return null;

        if (dto.Status is not null && dto.Status != userBook.Status)
        {
            userBook.Status = dto.Status.Value;

            if (dto.Status == BookStatus.Reading)
            {
                if (userBook.StartedAt is null) userBook.StartedAt = DateTime.UtcNow;
                userBook.FinishedAt = null;
            }
            else if (dto.Status == BookStatus.Read && userBook.FinishedAt is null)
                userBook.FinishedAt = DateTime.UtcNow;
            else if (dto.Status == BookStatus.ToRead)
            {
                userBook.StartedAt = null;
                userBook.FinishedAt = null;
            }
        }

        if (dto.Rating is not null) userBook.Rating = dto.Rating.Value;
        if (dto.PagesRead is not null) userBook.PagesRead = dto.PagesRead.Value;
        if (dto.Notes is not null) userBook.Notes = dto.Notes;

        userBook.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return BookMapper.ToUserBookDTO(userBook);
    }
}