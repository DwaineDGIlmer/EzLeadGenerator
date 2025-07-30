using Application.Extensions;
using Application.Models;
using Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public class LocalJobsRepositoryStoreTest
{
    private readonly string _testDirectory;
    private readonly Mock<ILogger<LocalJobsRepositoryStore>> _loggerMock;
    private readonly IOptions<SerpApiSettings> _options;

    public LocalJobsRepositoryStoreTest()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        _loggerMock = new Mock<ILogger<LocalJobsRepositoryStore>>();
        var settings = new SerpApiSettings { FileJobProfileDirectory = _testDirectory };
        _options = Options.Create(settings);
    }

    private LocalJobsRepositoryStore CreateStore() =>
        new(_options, _loggerMock.Object);

    private JobSummary CreateJob(string id, DateTime? postedDate = null) =>
        new()
        {
            JobId = id,
            PostedDate = postedDate ?? DateTime.UtcNow,
            // Add other required properties if needed
        };

    [Fact]
    public async Task AddJobAsync_SavesJobProfile()
    {
        var store = CreateStore();
        var job = CreateJob("job1");

        await store.AddJobAsync(job);

        var result = await store.GetJobsAsync("job1");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetJobsAsync_ById_ReturnsJobProfile()
    {
        var store = CreateStore();
        var job = CreateJob("job2");
        await store.AddJobAsync(job);

        var result = await store.GetJobsAsync("job2");

        Assert.NotNull(result);
        Assert.Equal("job2", result.JobId);
    }

    [Fact]
    public async Task GetJobsAsync_ById_retuns_Null()
    {
        var store = CreateStore();
        var result = await store.GetJobsAsync("missing");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetJobsAsync_ByDate_ReturnsFilteredJobs()
    {
        var store = CreateStore();
        var jobOld = CreateJob("jobOld", DateTime.UtcNow.AddDays(-10));
        var jobNew = CreateJob("jobNew", DateTime.UtcNow);

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

        var updated = await store.GetJobsAsync("job3");
        Assert.Equal(job.PostedDate.ToUniversalTime(), updated!.PostedDate.ToUniversalTime());
    }

    [Fact]
    public async Task UpdateJobAsync_CreatesIfNotExists()
    {
        var store = CreateStore();
        var job = CreateJob("job4");
        await store.UpdateJobAsync(job);

        var result = await store.GetJobsAsync("job4");
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

        // Should not throw
    }
}