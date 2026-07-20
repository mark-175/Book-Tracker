using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public interface IDbBookService
{
    public Task<BookSearchResult> FindBookInDb(string query, Guid userId);
}