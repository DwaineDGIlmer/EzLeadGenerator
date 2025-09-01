using Application.Contracts;
using Application.Models;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UnitTests.Services;

public sealed class DisplayRepositoryTest
{
    private readonly Mock<ICompanyRepository> _companyRepoMock;
    private readonly Mock<IJobsRepository> _jobsRepoMock;
    private readonly Mock<ILogger<DisplayRepository>> _loggerMock;

    public DisplayRepositoryTest()
    {
        _companyRepoMock = new Mock<ICompanyRepository>();
        _jobsRepoMock = new Mock<IJobsRepository>();
        _loggerMock = new Mock<ILogger<DisplayRepository>>();
    }

    private DisplayRepository CreateRepository(
        IEnumerable<JobSummary>? jobs = null,
        IEnumerable<CompanyProfile>? companies = null)
    {
        _jobsRepoMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(jobs ?? []);

        var companyList = companies?.ToList() ?? [];
        _companyRepoMock.Setup(r => r.GetCompanyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => companyList.FirstOrDefault(c => c.Id == id));

        return new DisplayRepository(_companyRepoMock.Object, _jobsRepoMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task GetPaginatedJobsAsync_ReturnsCorrectPage()
    {
        var jobs = Enumerable.Range(1, 10)
            .Select(i => new JobSummary { PostedDate = DateTime.Now.AddDays(-i), Id = i.ToString() })
            .ToList();
        var repo = CreateRepository(jobs);

        // Wait for background loading to complete
        await Task.Delay(100);

        var result = await repo.GetPaginatedJobsAsync(DateTime.Now.AddDays(-15), 1, 5);
    }

    [Fact]
    public async Task GetPaginatedCompaniesAsync_ReturnsCorrectPage()
    {
        var companies = Enumerable.Range(1, 8)
            .Select(i => new CompanyProfile(new JobSummary { CompanyId = i.ToString() }, new HierarchyResults()) { UpdatedAt = DateTime.Now.AddDays(-i), Id = i.ToString() })
            .ToList();
        var jobs = companies.Select(c => new JobSummary { CompanyId = c.Id, PostedDate = c.UpdatedAt, Id = c.Id }).ToList();
        var repo = CreateRepository(jobs, companies);

        await Task.Delay(100);

        var result = await repo.GetPaginatedCompaniesAsync(DateTime.Now.AddDays(-10), 1, 4);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public void Constructor_ThrowsIfNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(null!, _jobsRepoMock.Object, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(_companyRepoMock.Object, null!, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(_companyRepoMock.Object, _jobsRepoMock.Object, null!));
    }

    [Fact]
    public void LoadAllCompanies_AddsUniqueCompanyProfiles()
    {
        // Arrange
        var jobs = new List<JobSummary>
        {
            new() { CompanyId = "1" },
            new() { CompanyId = "2" },
            new() { CompanyId = "1" } // duplicate company id
        };
        var companyProfiles = new List<CompanyProfile>
        {
            new(new JobSummary { CompanyId = "1" }, new HierarchyResults()) { Id = "1", CompanyId = "1" },
            new(new JobSummary { CompanyId = "2" }, new HierarchyResults()) { Id = "2", CompanyId = "2" }
        };
        var allCompanies = new List<CompanyProfile>();
        var loggerMock = new Mock<ILogger>();
        var companyRepoMock = new Mock<ICompanyRepository>();
        companyRepoMock.Setup(r => r.GetCompanyProfileAsync("1")).ReturnsAsync(companyProfiles[0]);
        companyRepoMock.Setup(r => r.GetCompanyProfileAsync("2")).ReturnsAsync(companyProfiles[1]);

        // Act
        DisplayRepository.LoadAllCompanies(companyRepoMock.Object, jobs, allCompanies, loggerMock.Object);

        // Assert
        Assert.Equal(2, allCompanies.Count);
        Assert.Contains(allCompanies, c => c.CompanyId == "1");
        Assert.Contains(allCompanies, c => c.CompanyId == "2");
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully loaded")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LoadAllCompanies_LogsErrorOnException()
    {
        // Arrange
        var jobs = new List<JobSummary>
        {
            new() { CompanyId = "1" }
        };
        var allCompanies = new List<CompanyProfile>();
        var loggerMock = new Mock<ILogger>();
        var companyRepoMock = new Mock<ICompanyRepository>();
        companyRepoMock.Setup(r => r.GetCompanyProfileAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        DisplayRepository.LoadAllCompanies(companyRepoMock.Object, jobs, allCompanies, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error loading company profiles")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LoadAllJobs_AddsJobsAndLogsInformation()
    {
        // Arrange
        var jobs = new List<JobSummary>
        {
            new() { Id = "1" },
            new() { Id = "2" }
        };
        var allJobs = new List<JobSummary>() { new() { Id = "0" } };
        var loggerMock = new Mock<ILogger>();
        var jobsRepoMock = new Mock<IJobsRepository>();
        jobsRepoMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>())).ReturnsAsync(jobs);

        // Act
        DisplayRepository.LoadAllJobs(jobsRepoMock.Object, allJobs, loggerMock.Object);

        // Assert
        Assert.Equal(3, allJobs.Count);
        Assert.Contains(allJobs, j => j.Id == "0");
        Assert.Contains(allJobs, j => j.Id == "1");
        Assert.Contains(allJobs, j => j.Id == "2");
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully loaded")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LoadAllJobs_LogsErrorOnException()
    {
        // Arrange
        var allJobs = new List<JobSummary>();
        var loggerMock = new Mock<ILogger>();
        var jobsRepoMock = new Mock<IJobsRepository>();
        jobsRepoMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        DisplayRepository.LoadAllJobs(jobsRepoMock.Object, allJobs, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error loading company profiles")),
            It.Is<AggregateException>(ex => ex.InnerException != null && ex.InnerException.Message == "Test exception"),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}