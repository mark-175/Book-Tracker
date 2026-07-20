using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public class GoogleBookService : IGoogleBookService
{
    public Task<BookSearchResult> FindBookInGoogle(string query)
    {
        throw new NotImplementedException();
    }
}