using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApp.Pages;

namespace UnitTests.Pages;

public class IndexModelTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IMemoryCache _memoryCache;

    public IndexModelTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchApiKey"))]).Returns("dummy-api-key");
        _mockConfig.Setup(c => c[It.Is<string>(s => s.Contains("SearchEndpoint"))]).Returns("https://example.com/api/jobs");
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task OnPostAsync_QueryTooShort_SetsQueryTooShort()
    {
        // Arrange
        var model = new IndexModel(_mockConfig.Object, _memoryCache)
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
        var model = new IndexModel(_mockConfig.Object, _memoryCache)
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