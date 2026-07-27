using BookTracker.Api.Config;
using BookTracker.Api.DTOs;
using BookTracker.Api.DTOs.Google;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services.Google;

public class GoogleBookService : IGoogleBookService
{
    private readonly GoogleBooksApiOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleBookService> _logger;

    public GoogleBookService(
           IOptions<GoogleBooksApiOptions> options,
           HttpClient httpClient,
           ILogger<GoogleBookService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GoogleBooksSearchResponse?> FindBookInGoogle(string query, List<string> preferredLanguages)
    {
        try
        {
            var url = $"{_options.BaseUrl}?q={Uri.EscapeDataString(query)}&langRestrict={string.Join("&", preferredLanguages)}&key={_options.ApiKey}";
            var response = await _httpClient.GetFromJsonAsync<GoogleBooksSearchResponse>(url);
            return response;
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Google Books API request failed for query {Query} (status {StatusCode})",
                query, e.StatusCode);
            return null;
        }
    }
}