using Application.Models;
using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Application.UnitTests.Services;

public class SerpApiSearchServiceTest
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
    public async Task FetchOrganicResults_ReturnsCachedResults_IfAvailable()
    {
        // Arrange
        var organicResults = new List<OrganicResult> { new() { Title = "Test" } };
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()))
            .ReturnsAsync(organicResults);

        var service = new SerpApiSearchService(
            _optionsMock.Object,
            _cacheServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.FetchOrganicResults("test query", "United States");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Test", result!.First().Title);
        _cacheServiceMock.Verify(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FetchOrganicResults_PerformsHttpRequest_IfNoCache()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<OrganicResult>?)null);

        var organicResults = new List<OrganicResult> { new() { Title = "Result" } };
        var googleSearchResult = new GoogleSearchResult { OrganicResults = organicResults };
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
        var result = await service.FetchOrganicResults("test query", "United States");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Result", result!.First().Title);
    }

    [Fact]
    public async Task FetchOrganicResults_ReturnsNull_OnHttpFailure()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<OrganicResult>?)null);

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

        var result = await service.FetchOrganicResults("test query", "United States");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchOrganicResults_ReturnsNull_IfContentIsEmpty()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<OrganicResult>?)null);

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

        var result = await service.FetchOrganicResults("test query", "United States");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchOrganicResults_ReturnsNull_IfDeserializationFails()
    {
        _cacheServiceMock.Setup(c => c.TryGetAsync<IEnumerable<OrganicResult>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<OrganicResult>?)null);

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

        var result = await service.FetchOrganicResults("test query", "United States");

        Assert.Null(result);
    }
}