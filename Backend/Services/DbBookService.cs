using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public class DbBookService : IDbBookService
{
    public Task<BookSearchResult> FindBookInDb(string query, Guid userId)
    {
        throw new NotImplementedException();
    }
}