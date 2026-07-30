namespace BookTracker.Api.Enums;

public enum AddBookStatus
{
    Success,
    BookNotFound,
    UserNotFound,
    UnexpectedError,
    AlreadyInLibrary
}