using BookTracker.Api.DTOs;
using BookTracker.Api.DTOs.Google;
using BookTracker.Api.Entities;
using BookTracker.Api.Enums;

namespace BookTracker.Api.Tests.DTOs;

public class BookMapperTests
{
    [Fact]
    public void ToBook_FromManualDto_SetsManualSourceAndNullGoogleBooksId()
    {
        var dto = new AddManualBookDTO
        {
            Title = "Dune",
            Authors = "Frank Herbert",
            Isbn10 = "0441013597",
            Isbn13 = "9780441013593",
            PageCount = 412,
            Description = "A desert planet."
        };

        var book = BookMapper.ToBook(dto);

        Assert.Null(book.GoogleBooksId);
        Assert.Equal(BookSource.Manual, book.Source);
        Assert.Equal("Dune", book.Title);
        Assert.Equal("Frank Herbert", book.Authors);
        Assert.Equal("0441013597", book.Isbn10);
        Assert.Equal("9780441013593", book.Isbn13);
        Assert.Equal(412, book.PageCount);
        Assert.Equal("A desert planet.", book.Description);
    }

    [Fact]
    public void ToBook_FromManualDto_DefaultsSubtitleToEmptyWhenNull()
    {
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert", Subtitle = null };

        var book = BookMapper.ToBook(dto);

        Assert.Equal(string.Empty, book.Subtitle);
    }

    [Fact]
    public void ToBook_FromManualDto_LeavesOptionalFieldsNullWhenNotProvided()
    {
        var dto = new AddManualBookDTO { Title = "Dune", Authors = "Frank Herbert" };

        var book = BookMapper.ToBook(dto);

        Assert.Null(book.Isbn10);
        Assert.Null(book.Isbn13);
        Assert.Null(book.PageCount);
        Assert.Null(book.Description);
        Assert.Equal(string.Empty, book.Language);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_MapsFieldsAndSetsGoogleSource()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo
            {
                Title = "Dune",
                Subtitle = "A Novel",
                Authors = ["Frank Herbert", "Someone Else"],
                Language = "en",
                Description = "A desert planet.",
                PageCount = 412,
                ImageLinks = new GoogleImageLinks { Thumbnail = "https://example.com/cover.jpg" },
                IndustryIdentifiers =
                [
                    new GoogleIndustryIdentifier { Type = "ISBN_10", Identifier = "0441013597" },
                    new GoogleIndustryIdentifier { Type = "ISBN_13", Identifier = "9780441013593" }
                ]
            }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Equal("abc123", book.GoogleBooksId);
        Assert.Equal(BookSource.Google, book.Source);
        Assert.Equal("Dune", book.Title);
        Assert.Equal("A Novel", book.Subtitle);
        Assert.Equal("Frank Herbert, Someone Else", book.Authors);
        Assert.Equal("en", book.Language);
        Assert.Equal("A desert planet.", book.Description);
        Assert.Equal(412, book.PageCount);
        Assert.Equal("https://example.com/cover.jpg", book.CoverUrl);
        Assert.Equal("0441013597", book.Isbn10);
        Assert.Equal("9780441013593", book.Isbn13);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_DefaultsSubtitleAndAuthorsAndLanguageWhenNull()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo
            {
                Title = "Dune",
                Subtitle = null,
                Authors = null,
                Language = null
            }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Equal(string.Empty, book.Subtitle);
        Assert.Equal(string.Empty, book.Authors);
        Assert.Equal(string.Empty, book.Language);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_CoverUrlNullWhenImageLinksNull()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo { Title = "Dune", ImageLinks = null }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Null(book.CoverUrl);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_OnlyIsbn10Present_Isbn13StaysNull()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo
            {
                Title = "Dune",
                IndustryIdentifiers = [new GoogleIndustryIdentifier { Type = "ISBN_10", Identifier = "0441013597" }]
            }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Equal("0441013597", book.Isbn10);
        Assert.Null(book.Isbn13);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_OnlyIsbn13Present_Isbn10StaysNull()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo
            {
                Title = "Dune",
                IndustryIdentifiers = [new GoogleIndustryIdentifier { Type = "ISBN_13", Identifier = "9780441013593" }]
            }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Null(book.Isbn10);
        Assert.Equal("9780441013593", book.Isbn13);
    }

    [Fact]
    public void ToBook_FromGoogleVolume_NullIndustryIdentifiers_BothIsbnsStayNull()
    {
        var volume = new GoogleVolume
        {
            Id = "abc123",
            VolumeInfo = new GoogleVolumeInfo { Title = "Dune", IndustryIdentifiers = null }
        };

        var book = BookMapper.ToBook(volume);

        Assert.Null(book.Isbn10);
        Assert.Null(book.Isbn13);
    }

    [Fact]
    public void ToSearchResult_FromBook_MapsAllFields()
    {
        var book = new Book
        {
            Id = 7,
            Title = "Dune",
            Subtitle = "A Novel",
            Authors = "Frank Herbert",
            Language = "en",
            Description = "A desert planet.",
            CoverUrl = "https://example.com/cover.jpg",
            Isbn10 = "0441013597",
            Isbn13 = "9780441013593",
            PageCount = 412
        };

        var result = BookMapper.ToSearchResult(book);

        Assert.Equal(7, result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal("A Novel", result.Subtitle);
        Assert.Equal("Frank Herbert", result.Authors);
        Assert.Equal("en", result.Language);
        Assert.Equal("A desert planet.", result.Description);
        Assert.Equal("https://example.com/cover.jpg", result.CoverUrl);
        Assert.Equal("0441013597", result.Isbn10);
        Assert.Equal("9780441013593", result.Isbn13);
        Assert.Equal(412, result.PageCount);
    }

    [Fact]
    public void ToBookDTO_FromBook_MapsAllFields()
    {
        var book = new Book
        {
            Id = 7,
            Title = "Dune",
            Subtitle = "A Novel",
            Authors = "Frank Herbert",
            Language = "en",
            Description = "A desert planet.",
            CoverUrl = "https://example.com/cover.jpg",
            Isbn10 = "0441013597",
            Isbn13 = "9780441013593",
            PageCount = 412
        };

        var dto = BookMapper.ToBookDTO(book);

        Assert.Equal("Dune", dto.Title);
        Assert.Equal("A Novel", dto.Subtitle);
        Assert.Equal("Frank Herbert", dto.Authors);
        Assert.Equal("en", dto.Language);
        Assert.Equal("A desert planet.", dto.Description);
        Assert.Equal("https://example.com/cover.jpg", dto.CoverUrl);
        Assert.Equal("0441013597", dto.Isbn10);
        Assert.Equal("9780441013593", dto.Isbn13);
        Assert.Equal(412, dto.PageCount);
    }

    [Fact]
    public void ToUserBookDTO_FromUserBook_MapsCatalogAndLibraryFields()
    {
        var book = new Book
        {
            Id = 7,
            Title = "Dune",
            Subtitle = "A Novel",
            Authors = "Frank Herbert",
            Language = "en",
            Description = "A desert planet.",
            CoverUrl = "https://example.com/cover.jpg",
            Isbn10 = "0441013597",
            Isbn13 = "9780441013593",
            PageCount = 412
        };
        var startedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var finishedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var addedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        var userBook = new UserBook
        {
            Id = 99,
            Book = book,
            BookId = book.Id,
            Status = BookStatus.Reading,
            Rating = 4.5,
            PagesRead = 200,
            Notes = "Great so far",
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            AddedAt = addedAt,
            UpdatedAt = updatedAt
        };

        var dto = BookMapper.ToUserBookDTO(userBook);

        Assert.Equal(book.Id, dto.BookId);
        Assert.Equal("Dune", dto.Title);
        Assert.Equal("A Novel", dto.Subtitle);
        Assert.Equal("Frank Herbert", dto.Authors);
        Assert.Equal("en", dto.Language);
        Assert.Equal("A desert planet.", dto.Description);
        Assert.Equal("https://example.com/cover.jpg", dto.CoverUrl);
        Assert.Equal("0441013597", dto.Isbn10);
        Assert.Equal("9780441013593", dto.Isbn13);
        Assert.Equal(412, dto.PageCount);
        Assert.Equal(BookStatus.Reading, dto.Status);
        Assert.Equal(4.5, dto.Rating);
        Assert.Equal(200, dto.PagesRead);
        Assert.Equal("Great so far", dto.Notes);
        Assert.Equal(startedAt, dto.StartedAt);
        Assert.Equal(finishedAt, dto.FinishedAt);
        Assert.Equal(addedAt, dto.AddedAt);
        Assert.Equal(updatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void ToUserBookDTO_WhenNotYetStartedOrFinished_LeavesNullableDatesNull()
    {
        var book = new Book { Id = 3, Title = "Dune", Authors = "Frank Herbert" };
        var userBook = new UserBook
        {
            Book = book,
            BookId = book.Id,
            Status = BookStatus.ToRead,
            StartedAt = null,
            FinishedAt = null
        };

        var dto = BookMapper.ToUserBookDTO(userBook);

        Assert.Null(dto.StartedAt);
        Assert.Null(dto.FinishedAt);
    }
}
