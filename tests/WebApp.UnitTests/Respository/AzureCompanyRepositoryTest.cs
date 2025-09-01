using Application.Configurations;
using Application.Models;
using Application.Services;
using Azure;
using Azure.Data.Tables;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public sealed class AzureCompanyRepositoryTest
{
    private readonly Mock<TableClient> _tableClientMock = new();
    private readonly Mock<IOptions<AzureSettings>> _azOptionsMock = new();
    private readonly Mock<IOptions<EzLeadSettings>> _ezOptionsMock = new();
    private readonly Mock<ILogger<AzureCompanyRepository>> _loggerMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new(); // Add cache service mock
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

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
        _cacheServiceMock.Setup(x => x.TryGetAsync<CompanyProfile>(It.IsAny<string>())).ReturnsAsync((CompanyProfile?)null);
        _tableClientMock
            .Setup(x => x.GetEntityAsync<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), null, default))
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var result = await repo.GetCompanyProfileAsync("company1");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsProfile_FromCache()
    {
        var repo = CreateRepository();
        var companyName = "TestCompany";
        var companyId = "TestCompany";
        var cachedProfile = new CompanyProfile(new JobSummary() { CompanyName = companyName }, new HierarchyResults()) { CompanyId = companyId };
        var cacheKey = WebApp.Extensions.Extensions.GetCacheKey("Company", companyId);
        _cacheServiceMock.Setup(x => x.TryGetAsync<CompanyProfile>(cacheKey)).ReturnsAsync(cachedProfile);

        var result = await repo.GetCompanyProfileAsync(companyId);

        Assert.NotNull(result);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal(companyName, result.CompanyName);
        _tableClientMock.Verify(x => x.GetEntityAsync<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), null, default), Times.Never);
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

    private AzureCompanyRepository CreateRepository(string partitionKey = "TestPartition")
    {
        _azOptionsMock.Setup(o => o.Value).Returns(new AzureSettings { CompanyProfilePartionKey = partitionKey });
        return new AzureCompanyRepository(
            _tableClientMock.Object,
            _cacheServiceMock.Object, // Pass cache service mock
            _azOptionsMock.Object,
            _ezOptionsMock.Object,
            _loggerMock.Object
        );
    }
}