using System.Net;
using System.Text;
using System.Text.Json;
using BookTracker.Api.Config;
using BookTracker.Api.DTOs.Google;
using BookTracker.Api.Services.Google;
using BookTracker.Api.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BookTracker.Api.Tests.Services.Google;

public class GoogleBookServiceTests
{
    private static IOptions<GoogleBooksApiOptions> BuildOptions()
    {
        return Options.Create(new GoogleBooksApiOptions
        {
            BaseUrl = "https://example.com/volumes",
            ApiKey = "test-key"
        });
    }

    private static GoogleBookService BuildService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler);
        return new GoogleBookService(BuildOptions(), httpClient, Mock.Of<ILogger<GoogleBookService>>());
    }

    private static HttpResponseMessage JsonResponse(GoogleBooksSearchResponse response)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
        };
    }

    private static GoogleVolume BuildVolume(string id, string title, string? language)
    {
        return new GoogleVolume
        {
            Id = id,
            VolumeInfo = new GoogleVolumeInfo
            {
                Title = title,
                Language = language
            }
        };
    }

    [Fact]
    public async Task FindBookInGoogle_PreferredLanguageGiven_FiltersItemsByLanguageCaseInsensitively()
    {
        var response = new GoogleBooksSearchResponse
        {
            TotalItems = 3,
            Items =
            [
                BuildVolume("1", "English Book", "en"),
                BuildVolume("2", "French Book", "fr"),
                BuildVolume("3", "Uppercase English Book", "EN")
            ]
        };
        var service = BuildService(_ => JsonResponse(response));

        var result = await service.FindBookInGoogle("query", ["en"]);

        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalItems);
        Assert.Equal(["1", "3"], result.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task FindBookInGoogle_PreferredLanguagesEmpty_ReturnsAllItemsUnfiltered()
    {
        var response = new GoogleBooksSearchResponse
        {
            TotalItems = 2,
            Items =
            [
                BuildVolume("1", "English Book", "en"),
                BuildVolume("2", "French Book", "fr")
            ]
        };
        var service = BuildService(_ => JsonResponse(response));

        var result = await service.FindBookInGoogle("query", []);

        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalItems);
        Assert.Equal(["1", "2"], result.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task FindBookInGoogle_PreferredLanguagesOnlyBlank_ReturnsAllItemsUnfiltered()
    {
        var response = new GoogleBooksSearchResponse
        {
            TotalItems = 3,
            Items =
            [
                BuildVolume("1", "English Book", "en"),
                BuildVolume("2", "French Book", "fr"),
                BuildVolume("3", "No Language Book", null)
            ]
        };
        var service = BuildService(_ => JsonResponse(response));

        var result = await service.FindBookInGoogle("query", [" ", "", "   "]);

        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalItems);
        Assert.Equal(["1", "2", "3"], result.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task FindBookInGoogle_VolumeWithNullLanguage_ExcludedWhenFilteringActive()
    {
        var response = new GoogleBooksSearchResponse
        {
            TotalItems = 2,
            Items =
            [
                BuildVolume("1", "English Book", "en"),
                BuildVolume("2", "No Language Book", null)
            ]
        };
        var service = BuildService(_ => JsonResponse(response));

        var result = await service.FindBookInGoogle("query", ["en"]);

        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalItems);
        Assert.Equal(["1"], result.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task FindBookInGoogle_HttpRequestExceptionThrown_ReturnsNullWithoutThrowing()
    {
        var service = BuildService(_ => throw new HttpRequestException("boom"));

        var result = await service.FindBookInGoogle("query", ["en"]);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindBookInGoogle_ResponseBodyIsNullLiteral_ReturnsNull()
    {
        var service = BuildService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.FindBookInGoogle("query", ["en"]);

        Assert.Null(result);
    }
}
