using Application.Models;
using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Application.UnitTests.Services;

public class SerpApiSearchJobsServiceTest
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<SerpApiSearchJobsService>> _loggerMock = new();
    private readonly SerpApiSettings _settings = new()
    {
        ApiKey = "test-api-key",
        HttpClientName = "test-client",
        Endpoint = "https://api.serpapi.com/search",
        CacheExpirationInMinutes = 10
    };

    private SerpApiSearchJobsService CreateService(HttpResponseMessage? response = null, GoogleJobsResult? cachedResult = null)
    {
        var options = Options.Create(_settings);

        var httpClientMock = new HttpClient(new MockHttpMessageHandler(response));
        _httpClientFactoryMock.Setup(f => f.CreateClient(_settings.HttpClientName)).Returns(httpClientMock);

        _cacheServiceMock
            .Setup(c => c.TryGetAsync<GoogleJobsResult>(It.IsAny<string>()))
            .ReturnsAsync(cachedResult);

        return new SerpApiSearchJobsService(_cacheServiceMock.Object, _httpClientFactoryMock.Object, options, _loggerMock.Object);
    }

    [Fact]
    public async Task FetchJobs_ReturnsCachedJobs_IfAvailable()
    {
        var cachedJobs = new GoogleJobsResult
        {
            JobsResults = [new JobResult { Title = "Cached Job" }]
        };
        var service = CreateService(null, cachedJobs);

        var result = await service.FetchJobs("developer", "NY");

        Assert.Single(result);
        Assert.Equal("Cached Job", result.First().Title);
    }

    [Fact]
    public async Task FetchJobs_ReturnsEmptyList_OnHttpFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var service = CreateService(response);

        var result = await service.FetchJobs("developer", "NY");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchJobs_ReturnsEmptyList_OnEmptyContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };
        var service = CreateService(response);

        var result = await service.FetchJobs("developer", "NY");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchJobs_ReturnsEmptyList_OnDeserializationError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json}")
        };
        var service = CreateService(response);

        var result = await service.FetchJobs("developer", "NY");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchJobs_ReturnsJobs_AndCachesResult()
    {
        var jobsResult = new GoogleJobsResult
        {
            JobsResults = [new JobResult { Title = "API Job" }]
        };
        var json = JsonSerializer.Serialize(jobsResult, new JsonSerializerOptions { PropertyNamingPolicy = null });
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(response);

        _cacheServiceMock.Setup(c => c.CreateEntryAsync(It.IsAny<string>(), It.IsAny<GoogleJobsResult>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await service.FetchJobs("developer", "NY");

        Assert.Single(result);
        Assert.Equal("API Job", result.First().Title);
        _cacheServiceMock.Verify();
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _response;
        public MockHttpMessageHandler(HttpResponseMessage? response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response ?? new HttpResponseMessage(HttpStatusCode.OK));
    }
}