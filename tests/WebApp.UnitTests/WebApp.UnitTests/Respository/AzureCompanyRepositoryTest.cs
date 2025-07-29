using Application.Configurations;
using Application.Models;
using Application.Services;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public class AzureCompanyRepositoryTest
{
    private readonly Mock<TableClient> _tableClientMock = new();
    private readonly Mock<IOptions<AzureSettings>> _optionsMock = new();
    private readonly Mock<ILogger<AzureCompanyRepository>> _loggerMock = new();
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    private AzureCompanyRepository CreateRepository(string partitionKey = "TestPartition")
    {
        _optionsMock.Setup(o => o.Value).Returns(new AzureSettings { CompanyProfilePartionKey = partitionKey });
        return new AzureCompanyRepository(_tableClientMock.Object, _optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ThrowsArgumentException_WhenCompanyIdIsNullOrEmpty()
    {
        var repo = CreateRepository();
        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetCompanyProfileAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetCompanyProfileAsync(""));
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsNull_WhenEntityNotFound()
    {
        var repo = CreateRepository();
        _tableClientMock
            .Setup(x => x.GetEntityAsync<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), null, default))
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var result = await repo.GetCompanyProfileAsync("company1");
        Assert.Null(result);
    }

    [Fact]
    public async Task AddCompanyProfileAsync_ThrowsArgumentNullException_WhenProfileIsNull()
    {
        var repo = CreateRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddCompanyProfileAsync(null!));
    }

    [Fact]
    public async Task AddCompanyProfileAsync_ThrowsArgumentException_WhenCompanyIdIsNullOrEmpty()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary() { CompanyName = "" }, new HierarchyResults()) { CompanyId = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => repo.AddCompanyProfileAsync(profile));
    }

    [Fact]
    public async Task AddCompanyProfileAsync_CallsUpsertEntityAsync()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary() { CompanyName = "company1" }, new HierarchyResults())
        {
            CompanyId = "company1",
            CompanyName = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _tableClientMock
            .Setup(x => x.UpsertEntityAsync(It.IsAny<TableEntity>(), TableUpdateMode.Merge, default))
            .ReturnsAsync(Mock.Of<Response>());

        await repo.AddCompanyProfileAsync(profile);

        _tableClientMock.Verify(x => x.UpsertEntityAsync(It.IsAny<TableEntity>(), TableUpdateMode.Merge, default), Times.Once);
    }

    [Fact]
    public async Task UpdateCompanyProfileAsync_ThrowsArgumentNullException_WhenProfileIsNull()
    {
        var repo = CreateRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateCompanyProfileAsync(null!));
    }

    [Fact]
    public async Task UpdateCompanyProfileAsync_ThrowsArgumentException_WhenCompanyIdIsNullOrEmpty()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary() { CompanyName = "" }, new HierarchyResults()) { CompanyId = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateCompanyProfileAsync(profile));
    }

    [Fact]
    public async Task UpdateCompanyProfileAsync_CallsUpdateEntityAsync()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary() { CompanyName = "company1" }, new HierarchyResults())
        {
            CompanyId = "company1",
            CompanyName = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _tableClientMock
            .Setup(x => x.UpdateEntityAsync(It.IsAny<TableEntity>(), ETag.All, TableUpdateMode.Replace, default))
            .ReturnsAsync(Mock.Of<Response>());

        await repo.UpdateCompanyProfileAsync(profile);

        _tableClientMock.Verify(x => x.UpdateEntityAsync(It.IsAny<TableEntity>(), ETag.All, TableUpdateMode.Replace, default), Times.Once);
    }

    [Fact]
    public async Task DeleteCompanyProfileAsync_ThrowsArgumentNullException_WhenProfileIsNull()
    {
        var repo = CreateRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteCompanyProfileAsync(null!));
    }

    [Fact]
    public async Task DeleteCompanyProfileAsync_ThrowsArgumentException_WhenCompanyIdIsNullOrEmpty()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary(), new HierarchyResults()) { CompanyId = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteCompanyProfileAsync(profile));
    }

    [Fact]
    public async Task DeleteCompanyProfileAsync_CallsDeleteEntityAsync()
    {
        var repo = CreateRepository();
        var profile = new CompanyProfile(new JobSummary(), new HierarchyResults()) { CompanyId = "company1" };

        _tableClientMock
            .Setup(x => x.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), ETag.All, default))
            .ReturnsAsync(Mock.Of<Response>());

        await repo.DeleteCompanyProfileAsync(profile);

        _tableClientMock.Verify(x => x.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), ETag.All, default), Times.Once);
    }

    // Custom AsyncPageable implementation for testing
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
            yield return Page<T>.FromValues(items, null, Mock.Of<Response>());
        }
    }
}