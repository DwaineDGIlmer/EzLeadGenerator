using Application.Models;
using Core.Contracts;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Extensions;

namespace WebApp.UnitTests.Extensions;

public class ExtensionsTest
{
    // GetJobAsync Tests
    [Fact]
    public async Task GetJobAsync_ReturnsJob_WhenJobExistsInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var jobId = "test-job-id";
        var expectedJob = new JobSummary { Id = jobId, JobTitle = "Test Job" };
        cacheServiceMock.Setup(cs => cs.TryGetAsync<JobSummary>(jobId))
            .ReturnsAsync(expectedJob);

        // Act
        var result = await cacheServiceMock.Object.GetJobAsync(jobId, loggerMock.Object);

        // Assert
        Assert.Equal(expectedJob, result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(jobId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task GetJobAsync_ReturnsNull_WhenJobDoesNotExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var jobId = "non-existent-job-id";
        cacheServiceMock.Setup(cs => cs.TryGetAsync<JobSummary>(jobId))
            .ReturnsAsync((JobSummary)null!);

        // Act
        var result = await cacheServiceMock.Object.GetJobAsync(jobId, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(jobId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task GetJobAsync_ThrowsArgumentNullException_WhenCacheServiceIsNull()
    {
        // Arrange
        ICacheService cacheService = null;
        var loggerMock = new Mock<ILogger>();
        var jobId = "test-job-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            cacheService.GetJobAsync(jobId, loggerMock.Object));
    }

    [Fact]
    public async Task GetJobAsync_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        ILogger logger = null;
        var jobId = "test-job-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            cacheServiceMock.Object.GetJobAsync(jobId, logger!));
    }

    // GetJobsAsync Tests
    [Fact]
    public async Task GetJobsAsync_ReturnsOrderedJobs_WhenJobsExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var fromDate = new DateTime(2025, 1, 1);
        var cacheKey = $"Jobs:{fromDate.GenHashString()}:{fromDate.Ticks}";

        var jobs = new List<JobSummary>
        {
            new JobSummary { Id = "job1", JobTitle = "Job 1", PostedDate = new DateTime(2025, 1, 1) },
            new JobSummary { Id = "job2", JobTitle = "Job 2", PostedDate = new DateTime(2025, 1, 2) }
        };

        cacheServiceMock.Setup(cs => cs.TryGetAsync<IEnumerable<JobSummary>>(cacheKey))
            .ReturnsAsync(jobs);

        // Act
        var result = await cacheServiceMock.Object.GetJobsAsync(fromDate, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobs.OrderByDescending(j => j.PostedDate), result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved jobs list from cache")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task GetJobsAsync_ReturnsNull_WhenNoJobsExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var fromDate = new DateTime(2025, 1, 1);
        var cacheKey = $"Jobs:{fromDate.GenHashString()}:{fromDate.Ticks}";

        cacheServiceMock.Setup(cs => cs.TryGetAsync<IEnumerable<JobSummary>>(cacheKey))
            .ReturnsAsync((IEnumerable<JobSummary>)null!);

        // Act
        var result = await cacheServiceMock.Object.GetJobsAsync(fromDate, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(cacheKey)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    // GetCompanyAsync Tests
    [Fact]
    public async Task GetCompanyAsync_ReturnsCompany_WhenCompanyExistsInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var companyId = "test-company-id";
        var expectedCompany = new CompanyProfile { Id = companyId, CompanyName = "Test Company" };
        cacheServiceMock.Setup(cs => cs.TryGetAsync<CompanyProfile>(companyId))
            .ReturnsAsync(expectedCompany);

        // Act
        var result = await cacheServiceMock.Object.GetCompanyAsync(companyId, loggerMock.Object);

        // Assert
        Assert.Equal(expectedCompany, result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(companyId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task GetCompanyAsync_ReturnsNull_WhenCompanyDoesNotExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var companyId = "non-existent-company-id";
        cacheServiceMock.Setup(cs => cs.TryGetAsync<CompanyProfile>(companyId))
            .ReturnsAsync((CompanyProfile)null!);

        // Act
        var result = await cacheServiceMock.Object.GetCompanyAsync(companyId, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(companyId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    // GetCompaniesAsync Tests
    [Fact]
    public async Task GetCompaniesAsync_ReturnsOrderedCompanies_WhenCompaniesExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var fromDate = new DateTime(2025, 1, 1);
        var cacheKey = $"Companies:{fromDate.GenHashString()}:{fromDate.Ticks}";

        var companies = new List<CompanyProfile>
        {
            new CompanyProfile { Id = "company1", CompanyName = "Company 1", CreatedAt = new DateTime(2025, 1, 1) },
            new CompanyProfile { Id = "company2", CompanyName = "Company 2", CreatedAt = new DateTime(2025, 1, 2) }
        };

        cacheServiceMock.Setup(cs => cs.TryGetAsync<IEnumerable<CompanyProfile>>(cacheKey))
            .ReturnsAsync(companies);

        // Act
        var result = await cacheServiceMock.Object.GetCompaniesAsync(fromDate, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(companies.OrderByDescending(c => c.CreatedAt), result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved company list from cache")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task GetCompaniesAsync_ReturnsNull_WhenNoCompaniesExistInCache()
    {
        // Arrange
        var cacheServiceMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger>();
        var fromDate = new DateTime(2025, 1, 1);
        var cacheKey = $"Companies:{fromDate.GenHashString()}:{fromDate.Ticks}";

        cacheServiceMock.Setup(cs => cs.TryGetAsync<IEnumerable<CompanyProfile>>(cacheKey))
            .ReturnsAsync((IEnumerable<CompanyProfile>)null!);

        // Act
        var result = await cacheServiceMock.Object.GetCompaniesAsync(fromDate, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(cacheKey)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}
