using BookTracker.Api.Config;
using BookTracker.Api.DTOs;
using BookTracker.Api.DTOs.Google;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services.Google;

public class GoogleBookService : IGoogleBookService
{
    private readonly GoogleBooksApiOptions _options;
    private readonly HttpClient _httpClient;

    public GoogleBookService(
           IOptions<GoogleBooksApiOptions> options,
           HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
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
            Console.WriteLine($"Error occured while sneding request to endpoint {e.TargetSite}: {e.Message}");
            Console.WriteLine($"Status Code: {e.StatusCode} {e.HttpRequestError}");
            return null;
        }
    }
}