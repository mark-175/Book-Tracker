using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public interface IGoogleBookService
{
    public Task<BookSearchResult> FindBookInGoogle(string query);
}