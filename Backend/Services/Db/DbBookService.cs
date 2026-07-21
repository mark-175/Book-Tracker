using BookTracker.Api.Data;
using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Services.Db;

public class DbBookService : IDbBookService
{
    private AppDbContext _dbContext;

    public DbBookService(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
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

    public async Task<Book> SaveOrGetExistingAsync(Book book)
    {
        var existing = await _dbContext.Books
            .FirstOrDefaultAsync(b => b.GoogleBooksId == book.GoogleBooksId);

        if (existing is not null)
            return existing;

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
        return book;
    }
}