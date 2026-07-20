using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;
public interface IBookService
{
    public Task<BookSearchResult> FindBook(string query, Guid userId);
}