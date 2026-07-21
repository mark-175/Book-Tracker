using BookTracker.Api.DTOs;
using BookTracker.Api.Services.Db;
using BookTracker.Api.Services.Google;

namespace BookTracker.Api.Services;

public class BookService : IBookService
{
    private IDbBookService _dbBookService;
    private IGoogleBookService _googleBookService;
    private IUserService _userService;

    public BookService(IDbBookService dbBookService, IGoogleBookService googleBookService, IUserService userService)
    {
        _dbBookService = dbBookService;
        _googleBookService = googleBookService;
        _userService = userService;
    }

    public async Task<List<BookSearchResult>> FindBook(string query, Guid userId)
    {
        var preferredLanguages = await _userService.GetPreferredLanguages(userId);

        var dbResult = await _dbBookService.FindBookInDb(query, userId, preferredLanguages);
        if (dbResult.Count > 0) return dbResult;

        var googleResponse = await _googleBookService.FindBookInGoogle(query, preferredLanguages);

        if (googleResponse is null) return [];

        var googleResult = new List<BookSearchResult>();
        foreach (var volume in googleResponse.Items)
        {
            var book = await _dbBookService.SaveOrGetExistingAsync(BookMapper.ToBook(volume));
            googleResult.Add(BookMapper.ToSearchResult(book));
        }

        return googleResult;
    }
}