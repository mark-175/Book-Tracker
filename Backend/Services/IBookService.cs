using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public interface IBookService
{
    public Task<List<BookSearchResult>> FindBook(string query, Guid userId);
}