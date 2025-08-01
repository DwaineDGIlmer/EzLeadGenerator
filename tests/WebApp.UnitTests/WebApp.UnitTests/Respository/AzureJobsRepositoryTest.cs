using Application.Configurations;
using Application.Models;
using Azure;
using Azure.Data.Tables;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public class AzureJobsRepositoryTest
{
    private readonly Mock<TableClient> _tableClientMock = new();
    private readonly Mock<ILogger<AzureJobsRepository>> _loggerMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new(); // Add cache service mock
    private readonly IOptions<AzureSettings> _options = Options.Create(new AzureSettings { JobSummaryPartionKey = "TestPartition" });
    private AzureJobsRepository CreateRepository()
    => new(_tableClientMock.Object, _cacheServiceMock.Object, _options, _loggerMock.Object);

    [Fact]
    public async Task GetJobsAsync_ById_ReturnsJob_WhenFound()
    {
        var jobId = "job123";
        var job = new JobSummary { JobId = jobId, PostedDate = DateTime.UtcNow };
        var entity = new TableEntity(_options.Value.JobSummaryPartionKey, jobId)
        {
            { "Data", System.Text.Json.JsonSerializer.Serialize(job) }
        };
        var responseMock = Response.FromValue(entity, null!);

        _cacheServiceMock.Setup(x => x.TryGetAsync<JobSummary>(jobId)).ReturnsAsync((JobSummary?)null);
        _tableClientMock
            .Setup(c => c.GetEntityAsync<TableEntity>(_options.Value.JobSummaryPartionKey, jobId, null, default))
            .ReturnsAsync(responseMock);

        var repo = CreateRepository();
        var result = await repo.GetJobsAsync(jobId);

        Assert.NotNull(result);
        Assert.Equal(jobId, result!.JobId);
    }

    [Fact]
    public async Task GetJobsAsync_ById_ReturnsJob_FromCache()
    {
        var jobId = "cachedJob";
        var cachedJob = new JobSummary { JobId = jobId, PostedDate = DateTime.UtcNow };
        _cacheServiceMock.Setup(x => x.TryGetAsync<JobSummary>(jobId)).ReturnsAsync(cachedJob);

        var repo = CreateRepository();
        var result = await repo.GetJobsAsync(jobId);

        Assert.NotNull(result);
        Assert.Equal(jobId, result!.JobId);
        _tableClientMock.Verify(x => x.GetEntityAsync<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), null, default), Times.Never);
    }

    [Fact]
    public async Task GetJobsAsync_ById_ReturnsNull_WhenNotFound()
    {
        var jobId = "notfound";
        _tableClientMock
            .Setup(c => c.GetEntityAsync<TableEntity>(_options.Value.JobSummaryPartionKey, jobId, null, default))
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var repo = CreateRepository();
        var result = await repo.GetJobsAsync(jobId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetJobsAsync_ByDate_ReturnsJobs_AfterFromDate()
    {
        var fromDate = DateTime.UtcNow.AddDays(-1);
        var jobs = new[]
        {
            new JobSummary { JobId = "1", PostedDate = DateTime.UtcNow.AddHours(-2) },
            new JobSummary { JobId = "2", PostedDate = DateTime.UtcNow.AddHours(-1) }
        };
        var entities = jobs.Select(j =>
        {
            var e = new TableEntity(_options.Value.JobSummaryPartionKey, j.JobId)
            {
                ["Data"] = System.Text.Json.JsonSerializer.Serialize(j)
            };
            return e;
        });

        var asyncEnumerable = GetAsyncEnumerable(entities);

        _tableClientMock
            .Setup(c => c.QueryAsync<TableEntity>(It.IsAny<string>(), null, null, default))
            .Returns(asyncEnumerable);
        _cacheServiceMock.Setup(x => x.TryGetAsync<IEnumerable<JobSummary>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<JobSummary>?)null);

        var repo = CreateRepository();
        var result = await repo.GetJobsAsync(fromDate);

        Assert.Equal(2, result.Count());
        Assert.All(result, j => Assert.True(j.PostedDate >= fromDate));
    }

    [Fact]
    public async Task AddJobAsync_SavesEntity()
    {
        var job = new JobSummary { JobId = "add1", PostedDate = DateTime.UtcNow };
        _tableClientMock
            .Setup(c => c.UpsertEntityAsync(It.IsAny<TableEntity>(), TableUpdateMode.Merge, default))
            .Returns(Task.FromResult<Response>(null!))
            .Verifiable();

        var repo = CreateRepository();
        await repo.AddJobAsync(job);

        _tableClientMock.Verify();
    }

    [Fact]
    public async Task UpdateJobAsync_UpdatesEntity_WhenExists()
    {
        var job = new JobSummary { JobId = "update1", PostedDate = DateTime.UtcNow };
        var entity = new TableEntity(_options.Value.JobSummaryPartionKey, job.JobId)
        {
            { "CreatedAt", DateTime.UtcNow.ToString("o") }
        };
        var responseMock = Response.FromValue(entity, null!);

        _tableClientMock
            .Setup(c => c.GetEntityAsync<TableEntity>(_options.Value.JobSummaryPartionKey, job.JobId, null, default))
            .ReturnsAsync(responseMock);

        _tableClientMock
            .Setup(c => c.UpdateEntityAsync(It.IsAny<TableEntity>(), ETag.All, TableUpdateMode.Replace, default))
            .Returns(Task.FromResult<Response>(null!))
            .Verifiable();

        var repo = CreateRepository();
        await repo.UpdateJobAsync(job);

        _tableClientMock.Verify();
    }

    [Fact]
    public async Task UpdateJobAsync_DoesNotThrow_WhenNotFound()
    {
        var job = new JobSummary { JobId = "notfound", PostedDate = DateTime.UtcNow };
        _tableClientMock
            .Setup(c => c.GetEntityAsync<TableEntity>(_options.Value.JobSummaryPartionKey, job.JobId, null, default))
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var repo = CreateRepository();
        await repo.UpdateJobAsync(job);
    }

    [Fact]
    public async Task DeleteJobAsync_DeletesEntity()
    {
        var job = new JobSummary { JobId = "delete1", PostedDate = DateTime.UtcNow };
        _tableClientMock
            .Setup(c => c.DeleteEntityAsync(_options.Value.JobSummaryPartionKey, job.JobId, ETag.All, default))
            .Returns(Task.FromResult<Response>(null!))
            .Verifiable();

        var repo = CreateRepository();
        await repo.DeleteJobAsync(job);

        _tableClientMock.Verify();
    }

    private static AsyncPageable<TableEntity> GetAsyncEnumerable(IEnumerable<TableEntity> entities)
    {
        async IAsyncEnumerable<TableEntity> GetEntities()
        {
            foreach (var entity in entities)
                yield return entity;
            await Task.CompletedTask;
        }
        return new TestAsyncPageable<TableEntity>(GetEntities());
    }

    private class TestAsyncPageable<T>(IAsyncEnumerable<T> enumerable) : AsyncPageable<T> where T : notnull
    {
        private readonly IAsyncEnumerable<T> _enumerable = enumerable;

        public IAsyncEnumerable<T> AsAsyncEnumerable()
        {
            return _enumerable;
        }

        public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
        {
            var items = new List<T>();
            await foreach (var item in _enumerable)
            {
                items.Add(item);
            }
            yield return Page<T>.FromValues(items, null, null!);
        }
    }
}