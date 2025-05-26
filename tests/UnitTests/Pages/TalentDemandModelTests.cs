using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApp.Pages;

namespace UnitTests.Pages;

public class TalentDemandModelTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IMemoryCache _memoryCache;

    public TalentDemandModelTests()
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
        var model = new TalentDemandModel(_mockConfig.Object, _memoryCache)
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
        var model = new TalentDemandModel(_mockConfig.Object, _memoryCache)
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