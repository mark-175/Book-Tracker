using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public interface IBookService
{
    public Task<List<BookSearchResult>> FindBook(string query, Guid userId);
    public Task<AddBookToUserResult> AddBookToUser(int bookId, Guid userId);
    public Task<List<UserBookDTO>> GetUserBooks(Guid userId);
    public Task<UserBookDTO?> GetUserBook(Guid userId, int bookId);
}