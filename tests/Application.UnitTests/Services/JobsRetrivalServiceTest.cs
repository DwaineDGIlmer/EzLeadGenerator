using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Services;

sealed public class JobsRetrivalServiceTest
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<SerpApiSearchJobsService>> _loggerMock;
    private readonly IOptions<SerpApiSettings> _options;
    private readonly SerpApiSearchJobsService _service;

    public JobsRetrivalServiceTest()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<SerpApiSearchJobsService>>();
        _options = Options.Create(new SerpApiSettings
        {
            ApiKey = "test-api-key",
            HttpClientName = "test-client",
            Endpoint = "https://api.example.com/search"
        });

        _service = new SerpApiSearchJobsService(_cacheServiceMock.Object, _httpClientFactoryMock.Object, _options, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsIfArgumentsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SerpApiSearchJobsService(_cacheServiceMock.Object, _httpClientFactoryMock.Object, null!, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new SerpApiSearchJobsService(null!, _httpClientFactoryMock.Object, _options, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new SerpApiSearchJobsService(_cacheServiceMock.Object, null!, _options, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new SerpApiSearchJobsService(_cacheServiceMock.Object, _httpClientFactoryMock.Object, _options, null!));
    }
}