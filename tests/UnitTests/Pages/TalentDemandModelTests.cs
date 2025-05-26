using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;

namespace UnitTests.Pages;

public class TalentDemandModelTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<TalentDemandModel>> _mockLogger;

    public TalentDemandModelTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchApiKey"))]).Returns("dummy-api-key");
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchEndpoint"))]).Returns("https://example.com/api/jobs");
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<TalentDemandModel>>();

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
        var result = TalentDemandModel.GenerateRandomString(length);

        // Act & Assert
        Assert.Matches("^[A-Za-z0-9]+$", result);
    }

    [Fact]
    public void GenerateRandomString_ReturnsDifferentStrings()
    {
        // Arrange
        int length = 10;

        // Act
        var result1 = TalentDemandModel.GenerateRandomString(length);
        var result2 = TalentDemandModel.GenerateRandomString(length);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public async Task OnPostAsync_QueryTooShort_SetsQueryTooShort()
    {
        // Arrange
        var model = new TalentDemandModel(
            _mockConfig.Object,
            _memoryCache,
            _mockHttpClientFactory.Object,
            _mockLogger.Object)
        {
            JobTitle = "AI",
            Location = "NY"
        };

        // Setup TempData
        var tempData = new Mock<ITempDataDictionary>();
        model.TempData = tempData.Object;

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.True(model.QueryTooShort);
        Assert.Empty(model.GroupedJobResults);
    }

    [Fact]
    public async Task OnPostAsync_ValidQuery_DoesNotSetQueryTooShort()
    {
        // Arrange
        var model = new TalentDemandModel(
            _mockConfig.Object,
            _memoryCache,
            _mockHttpClientFactory.Object,
            _mockLogger.Object)
        {
            JobTitle = "AI Engineer",
            Location = "Remote",
            GroupedJobResults = new Dictionary<string, List<JobPosting>>()
            {
                { "dummy", new List<JobPosting>() { new() { } } }
            }
        };

        // Setup TempData
        var tempData = new Mock<ITempDataDictionary>();
        model.TempData = tempData.Object;

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.QueryTooShort);
    }

    [Fact]
    public void JobPosting_Defaults_AreEmptyStrings()
    {
        var job = new JobPosting();
        Assert.Equal(string.Empty, job.Title);
        Assert.Equal(string.Empty, job.Company);
        Assert.Equal(string.Empty, job.Location);
        Assert.Equal(string.Empty, job.Type);
        Assert.Equal(string.Empty, job.Posted);
        Assert.Equal(string.Empty, job.Description);
        Assert.Equal(string.Empty, job.ApplyLink);
    }
}