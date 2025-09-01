using Application.Configurations;
using Application.Models;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public sealed class LocalJobsRepositoryStoreTest
{
    private readonly string _testDirectory;
    private readonly Mock<ILogger<LocalJobsRepositoryStore>> _loggerMock;
    private readonly IOptions<EzLeadSettings> _options;
    private readonly Mock<ICacheService> _cacheServiceMock = new(); // Add cache service mock

    public LocalJobsRepositoryStoreTest()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        _loggerMock = new Mock<ILogger<LocalJobsRepositoryStore>>();
        var settings = new EzLeadSettings { FileJobProfileDirectory = _testDirectory };
        _options = Options.Create(settings);
    }

    [Fact]
    public async Task GetJobAsync_ById_ReturnsJob_FromCache()
    {
        var store = CreateStore();
        var jobId = "cachedJob";
        var cachedJob = CreateJob(jobId);
        var cacheKey = WebApp.Extensions.Extensions.GetCacheKey("Job", jobId);
        _cacheServiceMock.Setup(x => x.TryGetAsync<JobSummary>(cacheKey)).ReturnsAsync(cachedJob);

        var result = await store.GetJobAsync(jobId);

        Assert.NotNull(result);
        Assert.Equal(jobId, result.JobId);
        _cacheServiceMock.Verify(x => x.TryGetAsync<JobSummary>(cacheKey), Times.Once);
    }

    [Fact]
    public async Task AddJobAsync_SavesJobProfile()
    {
        var store = CreateStore();
        var job = CreateJob("job1");

        await store.AddJobAsync(job);

        var result = await store.GetJobAsync("job1");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetJobsAsync_ById_ReturnsJobProfile()
    {
        var store = CreateStore();
        var job = CreateJob("job2");
        await store.AddJobAsync(job);

        var result = await store.GetJobAsync("job2");

        Assert.NotNull(result);
        Assert.Equal("job2", result.JobId);
    }

    [Fact]
    public async Task GetJobAsync_ById_retuns_Null()
    {
        var store = CreateStore();
        var result = await store.GetJobAsync("missing");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetJobsAsync_ByDate_ReturnsFilteredJobs()
    {
        var store = CreateStore();
        var jobOld = CreateJob("jobOld", DateTime.UtcNow.AddDays(-10));
        var jobNew = CreateJob("jobNew", DateTime.UtcNow);

        _cacheServiceMock.Setup(x => x.TryGetAsync<IEnumerable<JobSummary>>(It.IsAny<string>()))
        .ReturnsAsync((IEnumerable<JobSummary>?)null);

        await store.AddJobAsync(jobOld);
        await store.AddJobAsync(jobNew);

        var jobs = await store.GetJobsAsync(DateTime.UtcNow.AddDays(-5));
        Assert.Contains(jobs, j => j.JobId == "jobNew");
        Assert.DoesNotContain(jobs, j => j.JobId == "jobOld");
    }

    [Fact]
    public async Task UpdateJobAsync_UpdatesExistingJob()
    {
        var store = CreateStore();
        var job = CreateJob("job3");
        await store.AddJobAsync(job);

        job.PostedDate = DateTime.UtcNow.AddDays(1);
        await store.UpdateJobAsync(job);

        var updated = await store.GetJobAsync("job3");
        Assert.Equal(job.PostedDate.ToUniversalTime(), updated!.PostedDate.ToUniversalTime());
    }

    [Fact]
    public async Task UpdateJobAsync_CreatesIfNotExists()
    {
        var store = CreateStore();
        var job = CreateJob("job4");
        await store.UpdateJobAsync(job);

        var result = await store.GetJobAsync("job4");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteJobAsync_RemovesJobProfile()
    {
        var store = CreateStore();
        var job = CreateJob("job5");
        await store.AddJobAsync(job);

        await store.DeleteJobAsync(job);

        string path = Path.Combine(_testDirectory, $"{job.JobId}.json");
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DeleteJobAsync_DoesNothingIfNotExists()
    {
        var store = CreateStore();
        var job = CreateJob("job6");
        await store.DeleteJobAsync(job);
    }


    [Fact]
    public async Task UpdateProperties_UpdatesPropertiesCorrectly()
    {
        // Arrange
        var jobId = "update1";
        var jobDirectory = _testDirectory;
        var logger = _loggerMock.Object;
        var options = new JsonSerializerOptions { WriteIndented = true };
        var originalJob = new JobSummary { JobId = jobId, PostedDate = DateTime.UtcNow.AddDays(-2), JobTitle = "Old Title" };
        var updatedJob = new JobSummary { JobId = jobId, PostedDate = DateTime.UtcNow, JobTitle = "New Title" };

        // Save original job to file
        var filePath = Path.Combine(jobDirectory, $"job.{jobId}.json");
        using (var stream = File.Create(filePath))
        {
            await JsonSerializer.SerializeAsync(stream, originalJob, options);
        }

        // Act
        var result = await LocalJobsRepositoryStore.UpdateProperties(updatedJob, jobDirectory, options, logger);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedJob.JobId, result.JobId);
        Assert.Equal(updatedJob.JobTitle, result.JobTitle);
        Assert.Equal(updatedJob.PostedDate, result.PostedDate);
        Assert.True(result.UpdatedAt > originalJob.PostedDate);
    }

    [Fact]
    public async Task UpdateProperties_Returns_Original_Job()
    {
        // Arrange
        var jobId = "corrupt";
        var jobDirectory = _testDirectory;
        var loggerMock = new Mock<ILogger>();
        var options = new JsonSerializerOptions { WriteIndented = true };
        var filePath = Path.Combine(jobDirectory, $"job.{jobId}.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");
        var job = new JobSummary { JobId = jobId };

        // Act
        var result = await LocalJobsRepositoryStore.UpdateProperties(job, jobDirectory, options, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobId, result.JobId);
    }

    [Fact]
    public async Task UpdateProperties_ThrowsDirectoryNotFoundException_WhenDirectoryMissing()
    {
        // Arrange
        var jobId = "missingdir";
        var missingDir = Path.Combine(_testDirectory, "doesnotexist");
        var logger = _loggerMock.Object;
        var options = new JsonSerializerOptions { WriteIndented = true };
        var job = new JobSummary { JobId = jobId };

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            LocalJobsRepositoryStore.UpdateProperties(job, missingDir, options, logger));
    }

    private LocalJobsRepositoryStore CreateStore() =>
        new(_options, _cacheServiceMock.Object, _loggerMock.Object);

    private static JobSummary CreateJob(string id, DateTime? postedDate = null) =>
        new()
        {
            JobId = id,
            PostedDate = postedDate ?? DateTime.UtcNow,
        };

}
