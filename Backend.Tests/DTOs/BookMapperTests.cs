using BookTracker.Api.DTOs;
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
}
