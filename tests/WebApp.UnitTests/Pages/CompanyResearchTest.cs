using Application.Contracts;
using Application.Models;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

sealed public class CompanyResearchModelTests
{
    private readonly Mock<IDisplayRepository> _repoMock;
    private readonly Mock<ILogger<CompanyResearchModel>> _loggerMock;

    public CompanyResearchModelTests()
    {
        _repoMock = new Mock<IDisplayRepository>();
        _loggerMock = new Mock<ILogger<CompanyResearchModel>>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CompanyResearchModel(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CompanyResearchModel(_repoMock.Object, null!));
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        var profiles = new List<CompanyProfile>
        {
            new() { CompanyName = "Test1" },
            new() { CompanyName = "Test2" }
        };
        _repoMock.Setup(r => r.GetJobCount(It.IsAny<DateTime>())).Returns(2);
        _repoMock.Setup(r => r.GetPaginatedCompanies(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(profiles);

        var model = new CompanyResearchModel(_repoMock.Object, _loggerMock.Object);

        Assert.Equal(2, model.TotalCount);
        Assert.Equal(2, model.CompanySummaries.Count);
        Assert.Equal(2, model.Profiles.Count);
        Assert.Equal("Test1", model.CompanySummaries[0].CompanyName);
        Assert.Equal(1, model.Profiles[0].CurrentIndex);
        Assert.Equal("Test2", model.CompanySummaries[1].CompanyName);
        Assert.Equal(2, model.Profiles[1].CurrentIndex);
    }

    [Fact]
    public async Task OnGetAsync_LogsWarning_WhenCompanySummariesIsNullOrEmpty()
    {
        _repoMock.Setup(r => r.GetJobCount(It.IsAny<DateTime>())).Returns(0);
        _repoMock.Setup(r => r.GetPaginatedCompanies(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<CompanyProfile>());

        var model = new CompanyResearchModel(_repoMock.Object, _loggerMock.Object);

        await model.OnGetAsync();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No company summaries found.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_DoesNotLogWarning_WhenCompanySummariesHasItems()
    {
        var profiles = new List<CompanyProfile>
        {
            new CompanyProfile { CompanyName = "Test1" }
        };
        _repoMock.Setup(r => r.GetJobCount(It.IsAny<DateTime>())).Returns(1);
        _repoMock.Setup(r => r.GetPaginatedCompanies(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(profiles);

        var model = new CompanyResearchModel(_repoMock.Object, _loggerMock.Object);

        await model.OnGetAsync();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}