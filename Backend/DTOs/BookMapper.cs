using BookTracker.Api.DTOs.Google;
using BookTracker.Api.Entities;

namespace BookTracker.Api.DTOs;

public static class BookMapper
{
    public static Book ToBook(GoogleVolume volume)
    {
        return new Book
        {
            GoogleBooksId = volume.Id,
            Title = volume.VolumeInfo.Title,
            Subtitle = volume.VolumeInfo.Subtitle ?? string.Empty,
            Authors = string.Join(", ", volume.VolumeInfo.Authors ?? []),
            Language = volume.VolumeInfo.Language ?? string.Empty,
            Description = volume.VolumeInfo.Description,
            CoverUrl = volume.VolumeInfo.ImageLinks?.Thumbnail,
            Isbn10 = volume.VolumeInfo.IndustryIdentifiers?
                .FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier,
            Isbn13 = volume.VolumeInfo.IndustryIdentifiers?
                .FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier,
            PageCount = volume.VolumeInfo.PageCount,
            CachedAt = DateTime.UtcNow
        };
    }

    public static BookSearchResult ToSearchResult(Book book)
    {
        return new BookSearchResult
        {
            Id = book.Id,
            Title = book.Title,
            Subtitle = book.Subtitle,
            Authors = book.Authors,
            Language = book.Language,
            Description = book.Description,
            CoverUrl = book.CoverUrl,
            Isbn10 = book.Isbn10,
            Isbn13 = book.Isbn13,
            PageCount = book.PageCount
        };
    }

    public static BookDTO ToBookDTO(Book book)
    {
        return new BookDTO
        {
            Title = book.Title,
            Subtitle = book.Subtitle,
            Authors = book.Authors,
            Language = book.Language,
            Description = book.Description,
            CoverUrl = book.CoverUrl,
            Isbn10 = book.Isbn10,
            Isbn13 = book.Isbn13,
            PageCount = book.PageCount,
        };
    }

    public static UserBookDTO ToUserBookDTO(UserBook userBook)
    {
        return new UserBookDTO
        {
            BookId = userBook.Book.Id,
            Title = userBook.Book.Title,
            Subtitle = userBook.Book.Subtitle,
            Authors = userBook.Book.Authors,
            Language = userBook.Book.Language,
            Description = userBook.Book.Description,
            CoverUrl = userBook.Book.CoverUrl,
            Isbn10 = userBook.Book.Isbn10,
            Isbn13 = userBook.Book.Isbn13,
            PageCount = userBook.Book.PageCount,
            Status = userBook.Status,
            Rating = userBook.Rating,
            PagesRead = userBook.PagesRead,
            Notes = userBook.Notes,
            StartedAt = userBook.StartedAt,
            FinishedAt = userBook.FinishedAt,
            AddedAt = userBook.AddedAt,
            UpdatedAt = userBook.UpdatedAt,
        };
    }
}
