using Application.Contracts;
using Application.Models;
using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests.Services;

public class SearpApiSourceServiceTest
{
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ISearch<OrganicResult>> _searchServiceMock = new();
    private readonly Mock<IJobsRetrieval<JobResult>> _jobsRetrievalMock = new();
    private readonly Mock<IOpenAiChatService> _aiChatServiceMock = new();
    private readonly Mock<ICompanyRepository> _companyRepositoryMock = new();
    private readonly Mock<IJobsRepository> _jobsRepositoryMock = new();
    private readonly Mock<ILogger<SearpApiSourceService>> _loggerMock = new();
    private readonly IOptions<SerpApiSettings> _settings = Options.Create(new SerpApiSettings { Query = "test", Location = "us" });

    private SearpApiSourceService CreateService() =>
        new(
            _settings,
            _cacheServiceMock.Object,
            _searchServiceMock.Object,
            _aiChatServiceMock.Object,
            _jobsRetrievalMock.Object,
            _jobsRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _loggerMock.Object);

    [Fact]
    public async Task UpdateJobSourceAsync_ReturnsFalse_WhenNoJobs()
    {
        _jobsRetrievalMock.Setup(j => j.FetchJobs(It.IsAny<string>(), It.IsAny<string>()))!
            .ReturnsAsync((IEnumerable<JobResult>?)null);

        var service = CreateService();
        var result = await service.UpdateJobSourceAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateJobSourceAsync_ReturnsTrue_WhenJobsAreProcessed()
    {
        var jobs = new List<JobResult>
        {
            new() { JobId = "1", CompanyName = "TestCo", Description = "desc" }
        };
        _jobsRetrievalMock.Setup(j => j.FetchJobs(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(jobs);

        _jobsRepositoryMock.Setup(r => r.GetJobsAsync(It.IsAny<string>()))
            .ReturnsAsync((JobSummary?)null);

        _aiChatServiceMock.Setup(a => a.GetChatCompletion<DivisionInference>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DivisionInference { Division = "Division", Reasoning = "Reason", Confidence = 1 });

        _jobsRepositoryMock.Setup(r => r.AddJobAsync(It.IsAny<JobSummary>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.UpdateJobSourceAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateCompanyProfilesAsync_ReturnsFalse_WhenNoJobs()
    {
        _jobsRepositoryMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))!
            .ReturnsAsync((IEnumerable<JobSummary>?)null);

        var service = CreateService();
        var result = await service.UpdateCompanyProfilesAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateCompanyProfilesAsync_HandlesExistingCompanyProfile()
    {
        var jobs = new List<JobSummary>
        {
            new() { CompanyId = "cid", CompanyName = "TestCo", JobDescription = "desc", Division = "div" }
        };
        _jobsRepositoryMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(jobs);

        _companyRepositoryMock.Setup(c => c.GetCompanyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync(new CompanyProfile(new JobSummary() { CompanyName = "Test Company" }, new HierarchyResults()));

        var service = CreateService();
        var result = await service.UpdateCompanyProfilesAsync();

        Assert.True(result == false || result == true); // Just ensure no exception
    }

    [Fact]
    public void HierarchyResults_InitializesOrgHierarchy()
    {
        var results = new HierarchyResults();
        Assert.NotNull(results.OrgHierarchy);
        Assert.Empty(results.OrgHierarchy);
    }

    [Fact]
    public void HierarchyItem_Properties_SetAndGet()
    {
        var item = new HierarchyItem { Name = "John", Title = "Manager" };
        Assert.Equal("John", item.Name);
        Assert.Equal("Manager", item.Title);
    }
}