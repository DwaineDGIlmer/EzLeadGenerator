using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;

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
    public void GenerateRandomString_ReturnsStringOfCorrectLength()
    {
        // Arrange
        int length = 10;

        // Act
        var result = IndexModel.GenerateRandomString(length);

        // Assert
        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void GenerateRandomString_ReturnsAlphanumericString()
    {
        // Arrange
        int length = 20;
        var result = IndexModel.GenerateRandomString(length);

        // Act & Assert
        Assert.Matches("^[A-Za-z0-9]+$", result);
    }

    [Fact]
    public void GenerateRandomString_ReturnsDifferentStrings()
    {
        // Arrange
        int length = 10;

        // Act
        var result1 = IndexModel.GenerateRandomString(length);
        var result2 = IndexModel.GenerateRandomString(length);

        // Assert
        Assert.NotEqual(result1, result2);
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