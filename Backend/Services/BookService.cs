using BookTracker.Api.DTOs;

namespace BookTracker.Api.Services;

public class BookService : IBookService
{
    private IDbBookService _dbBookService;
    private IGoogleBookService _googleBookService;

    public BookService(IDbBookService dbBookService, IGoogleBookService googleBookService)
    {
        _dbBookService = dbBookService;
        _googleBookService = googleBookService;
    }

    public Task<BookSearchResult> FindBook(string query, Guid userId)
    {
        // Find Book in database (_dbBookService) and return if found

        // If not found, query use _googleBookService to find

        // If found: 1. Add to database using _dbBookService 2. return

        // Else return null

        throw new NotImplementedException();
    }
}