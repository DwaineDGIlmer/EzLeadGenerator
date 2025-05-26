using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UnitTests.Pages;

public class IndexModelTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<IndexModel>> _mockLogger;

    public IndexModelTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchApiKey"))]).Returns("dummy-api-key");
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchEndpoint"))]).Returns("https://example.com/api/jobs");
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<IndexModel>>();

        // Setup HttpClientFactory to return a default HttpClient (or a mock if you want to control responses)
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
    }

    [Fact]
    public async Task OnPostAsync_QueryTooShort_SetsQueryTooShort()
    {
        // Arrange
        var model = new IndexModel(
            _mockConfig.Object,
            _memoryCache,
            _mockHttpClientFactory.Object,
            _mockLogger.Object)
        {
            SearchQuery = "ab"
        };

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.True(model.QueryTooShort);
        Assert.Empty(model.Results);
    }

    [Fact]
    public async Task OnPostAsync_NoPlatforms_ReturnsNoResults()
    {
        // Arrange
        var model = new IndexModel(
            _mockConfig.Object,
            _memoryCache,
            _mockHttpClientFactory.Object,
            _mockLogger.Object)
        {
            SearchQuery = "test",
            SelectedPlatforms = []
        };

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.Empty(model.Results);
    }

    [Fact]
    public void LeadInfo_Defaults_AreEmptyStrings()
    {
        var lead = new LeadInfo();
        Assert.Equal(string.Empty, lead.Email);
        Assert.Equal(string.Empty, lead.Name);
        Assert.Equal(string.Empty, lead.Platform);
        Assert.Equal(string.Empty, lead.Link);
        Assert.Equal(string.Empty, lead.Phone);
    }
}