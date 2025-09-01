using Application.Models;
using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Application.UnitTests.Services;

sealed public class SerpApiSearchServiceTest
{
    private readonly Mock<IOptions<SerpApiSettings>> _optionsMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SerpApiSearchService>> _loggerMock;
    private readonly SerpApiSettings _settings;

    public SerpApiSearchServiceTest()
    {
        _settings = new SerpApiSettings
        {
            ApiKey = "test-api-key",
            Endpoint = "https://serpapi.com/search",
            CacheExpirationInMinutes = 10,
            HttpClientName = "serpApiClient"
        };

        _optionsMock = new Mock<IOptions<SerpApiSettings>>();
        _optionsMock.Setup(o => o.Value).Returns(_settings);

        _cacheServiceMock = new Mock<ICacheService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SerpApiSearchService>>();
    }

    [Fact]
    public async Task FetchOrganicResults_PerformsHttpRequest_IfNoCache()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<GoogleSearchResult>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<GoogleSearchResult>?)null);

        var googleSearchResult = new GoogleSearchResult() { OrganicResults = [new() { Title = "Result" }] };
        var json = JsonSerializer.Serialize(googleSearchResult);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(httpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClient);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.FetchOrganicResults<OrganicResult>("test query", "United States");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Result", result!.First()!.Title);
    }

    [Fact]
    public async Task FetchSearchResults_ReturnsCachedResult_IfAvailable()
    {
        // Arrange
        var cachedResult = new GoogleSearchResult { OrganicResults = [] };
        _cacheServiceMock.Setup(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()))
            .ReturnsAsync(cachedResult);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.FetchSearchResults<GoogleSearchResult>("test query", "United States");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal(cachedResult, result!.First());
        _cacheServiceMock.Verify(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FetchSearchResults_PerformsHttpRequest_IfNoCache()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()))
            .ReturnsAsync((GoogleSearchResult?)null);

        var googleSearchResult = new GoogleSearchResult { OrganicResults = [] };
        var json = JsonSerializer.Serialize(googleSearchResult);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(httpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClient);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.FetchSearchResults<GoogleSearchResult>("test query", "United States");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!);
        _cacheServiceMock.Verify(c => c.CreateEntryAsync(It.IsAny<string>(), It.IsAny<GoogleSearchResult>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task FetchSearchResults_ReturnsNull_OnHttpFailure()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()))
            .ReturnsAsync((GoogleSearchResult?)null);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(httpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClient);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var result = await service.FetchSearchResults<GoogleSearchResult>("test query", "United States");

        Assert.Empty(result!);
    }

    [Fact]
    public async Task FetchSearchResults_ReturnsNull_IfContentIsEmpty()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()))
            .ReturnsAsync((GoogleSearchResult?)null);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        var httpClient = new HttpClient(httpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClient);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var result = await service.FetchSearchResults<GoogleSearchResult>("test query", "United States");

        Assert.Empty(result!);
    }

    [Fact]
    public async Task FetchSearchResults_ReturnsNull_IfDeserializationFails()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<GoogleSearchResult>(It.IsAny<string>()))
            .ReturnsAsync((GoogleSearchResult?)null);

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ invalid json }")
            });

        var httpClient = new HttpClient(httpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClient);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var result = await service.FetchSearchResults<GoogleSearchResult>("test query", "United States");

        Assert.Empty(result!);
    }
}