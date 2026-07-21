using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;

namespace BookTracker.Api.Services.Db;

public interface IDbBookService
{
    public Task<List<BookSearchResult>> FindBookInDb(string query, Guid userId, List<string> preferredLanguages);
    public Task<Book> SaveOrGetExistingAsync(Book book);
}