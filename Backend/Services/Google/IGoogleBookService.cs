using BookTracker.Api.DTOs;
using BookTracker.Api.DTOs.Google;

namespace BookTracker.Api.Services.Google;

public interface IGoogleBookService
{
    public Task<GoogleBooksSearchResponse?> FindBookInGoogle(string query, List<string> preferredLanguages);
}