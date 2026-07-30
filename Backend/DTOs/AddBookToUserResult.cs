using BookTracker.Api.Entities;
using BookTracker.Api.Enums;

namespace BookTracker.Api.DTOs;

public class AddBookToUserResult
{
    public AddBookStatus AddBookStatus { get; set; }
    public int BookId { get; set; }
    public BookDTO? Book { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;

    public static AddBookToUserResult UserNotFoundResult(int bookId) => new()
    {
        AddBookStatus = AddBookStatus.UserNotFound,
        BookId = bookId,
        Book = null,
        UserId = Guid.Empty,
        Message = "User Not found"
    };

    public static AddBookToUserResult UnexpectedError(int bookId) => new()
    {
        AddBookStatus = AddBookStatus.UnexpectedError,
        BookId = bookId,
        Book = null,
        UserId = Guid.Empty,
        Message = "Unexpected error occured"
    };

    public static AddBookToUserResult BookNotFoundResult(Guid userId, int bookId) => new()
    {
        AddBookStatus = AddBookStatus.BookNotFound,
        BookId = bookId,
        Book = null,
        UserId = userId,
        Message = "Book Not found"
    };

    public static AddBookToUserResult AlreadyInLibraryResult(Guid userId, int bookId) => new()
    {
        AddBookStatus = AddBookStatus.AlreadyInLibrary,
        BookId = bookId,
        Book = null,
        UserId = userId,
        Message = "Book is already in your library"
    };

    public static AddBookToUserResult Success(Guid userId, int bookId, BookDTO book) => new()
    {
        AddBookStatus = AddBookStatus.Success,
        BookId = bookId,
        Book = book,
        UserId = userId,
        Message = "Book added successfully"
    };
}