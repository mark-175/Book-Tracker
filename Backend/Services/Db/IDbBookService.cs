using BookTracker.Api.DTOs;
using BookTracker.Api.Entities;

namespace BookTracker.Api.Services.Db;

public interface IDbBookService
{
    public Task<List<BookSearchResult>> FindBookInDb(string query, Guid userId, List<string> preferredLanguages);
    public Task<Book> AddBookToDb(Book book);
    public Task<AddBookToUserResult> AddBookToUser(int bookId, Guid userId);
    public Task<List<UserBookDTO>> GetUserBooks(Guid userId);
    public Task<UserBookDTO?> GetUserBook(Guid userId, int bookId);
    public Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto);
}