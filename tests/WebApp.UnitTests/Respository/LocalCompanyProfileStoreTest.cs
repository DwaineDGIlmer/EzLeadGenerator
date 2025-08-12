using Application.Configurations;
using Application.Models;
using Application.Services;
using Core.Contracts;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public class LocalCompanyProfileStoreTest
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<LocalCompanyProfileStore>> _loggerMock = new();

    public LocalCompanyProfileStoreTest()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsProfile_WhenExists()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("get-company");
        await store.AddCompanyProfileAsync(profile);

        var result = await store.GetCompanyProfileAsync(profile.CompanyId);

        Assert.NotNull(result);
        Assert.Equal(profile.CompanyId, result.CompanyId);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsNull_WhenNotExists()
    {
        var store = CreateStore(out var _);

        var result = await store.GetCompanyProfileAsync("missing-company");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_FromDate_FiltersProfiles()
    {
        var store = CreateStore(out var _);
        var oldProfile = CreateProfile("old", DateTime.Now.AddDays(-1));
        var newProfile = CreateProfile("new", DateTime.Now);

        _cacheServiceMock.Setup(x => x.TryGetAsync<IEnumerable<CompanyProfile>>(It.IsAny<string>())).ReturnsAsync((IEnumerable<CompanyProfile>?)null);
        await store.AddCompanyProfileAsync(oldProfile);
        await store.AddCompanyProfileAsync(newProfile);

        var results = await store.GetCompanyProfileAsync(DateTime.Now.AddMinutes(-1));
        Assert.Contains(results, p => p.CompanyName == "new");
        Assert.DoesNotContain(results, p => p.CompanyName == "old");
    }

    [Fact]
    public async Task UpdateCompanyProfileAsync_UpdatesProfile()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("update-company");
        await store.AddCompanyProfileAsync(profile);

        profile.UpdatedAt = DateTime.Now.AddMinutes(5);
        await store.UpdateCompanyProfileAsync(profile);

        var updated = await store.GetCompanyProfileAsync(profile.CompanyId);
        Assert.NotNull(updated);
        Assert.NotEqual(updated.UpdatedAt, profile.CreatedAt);
    }

    [Fact]
    public async Task DeleteCompanyProfileAsync_DeletesProfileFile()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("delete-company");
        await store.AddCompanyProfileAsync(profile);

        await store.DeleteCompanyProfileAsync(profile);

        var result = await store.GetCompanyProfileAsync(profile.CompanyName);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCompanyProfileAsync_DoesNothing_WhenFileNotExists()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("nonexistent-company");

        await store.DeleteCompanyProfileAsync(profile);

        string path = Path.Combine(_testDirectory, $"{profile.CompanyId}.json");
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsProfile_FromCache()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("cached-company");
        _cacheServiceMock.Setup(x => x.TryGetAsync<CompanyProfile>(profile.CompanyId)).ReturnsAsync(profile);

        var result = await store.GetCompanyProfileAsync(profile.CompanyId);

        Assert.NotNull(result);
        Assert.Equal(profile.CompanyId, result.CompanyId);
    }

    [Fact]
    public async Task UpdateProperties_Hierarchy()
    {
        // Arrange
        var original = new CompanyProfile
        {
            CompanyId = "123",
            CompanyName = "Original",
            HierarchyResults = new HierarchyResults
            {
                OrgHierarchy =
                [
                    new HierarchyItem { Name = "John Smith", Title = "King" }
                ]
            },
            UpdatedAt = new DateTime(2000, 1, 1)
        };
        var updated = new CompanyProfile
        {
            CompanyId = "123",
            CompanyName = "Updated",
            HierarchyResults = new HierarchyResults
            {
                OrgHierarchy =
                [
                    new HierarchyItem { Name = "Jane Doe", Title = "Queen" }
                ]
            },
            UpdatedAt = new DateTime(2020, 1, 1)
        };

        string tempFile = LocalCompanyProfileStore.GetFilePath("123", Path.GetTempPath());
        await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(original));

        // Act
        var result = await LocalCompanyProfileStore.UpdateProperties(updated, Path.GetTempPath(), _loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
        Assert.NotNull(result?.HierarchyResults);
        Assert.True(result.HierarchyResults.OrgHierarchy.Count == 2);

        var resultHierarchy = result.HierarchyResults?.OrgHierarchy?.Where(r => r.Title == "Queen");
        Assert.NotNull(resultHierarchy);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task UpdateProperties_Throws_DirectoryNotFoundException()
    {
        var original = new CompanyProfile
        {
            CompanyId = "123",
            CompanyName = "Original",
            UpdatedAt = new DateTime(2000, 1, 1)
        };
        var result = await Assert.ThrowsAsync<DirectoryNotFoundException>(() => LocalCompanyProfileStore.UpdateProperties(original, "NotReal", _loggerMock.Object));
        Assert.Equal("The directory 'NotReal' does not exist.", result.Message);
    }

    [Fact]
    public async Task UpdateProperties_UpdatesWritableProperties()
    {
        // Arrange
        var original = new CompanyProfile
        {
            CompanyId = "123",
            CompanyName = "Original",
            UpdatedAt = new DateTime(2000, 1, 1)
        };
        var updated = new CompanyProfile
        {
            CompanyId = "123",
            CompanyName = "Updated",
            UpdatedAt = new DateTime(2020, 1, 1)
        };

        string tempFile = LocalCompanyProfileStore.GetFilePath("123", Path.GetTempPath());
        await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(original));

        // Act
        var result = await LocalCompanyProfileStore.UpdateProperties(updated, Path.GetTempPath(), _loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original", result.CompanyName); // The original from file
        Assert.Equal("Updated", updated.CompanyName); // The updated profile should keep its value
        Assert.True(updated.UpdatedAt > original.UpdatedAt); // UpdatedAt should be set to now

        File.Delete(tempFile);
    }

    [Fact]
    public async Task UpdateProperties_ReturnsNullIfFileIsEmpty()
    {
        // Arrange
        string companyId = Guid.NewGuid().ToString();
        string tempFile = LocalCompanyProfileStore.GetFilePath(companyId, Path.GetTempPath());
        await File.WriteAllTextAsync(tempFile, ""); // Empty file

        // Act
        var result = await LocalCompanyProfileStore.UpdateProperties(new() { CompanyId = companyId, CompanyName = "Test" }, Path.GetTempPath(), _loggerMock.Object);

        // Assert
        Assert.Null(result);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task UpdateProperties_IgnoresReadOnlyProperties()
    {
        // Arrange
        var original = new CompanyProfile
        {
            CompanyId = "0987654321",
            CompanyName = "Original"
        };
        var updated = new CompanyProfile
        {
            CompanyId = "1234567890",
            CompanyName = "Updated"
        };

        string tempFile = LocalCompanyProfileStore.GetFilePath(updated.CompanyId, Path.GetTempPath());
        await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(original));

        // Act
        var result = await LocalCompanyProfileStore.UpdateProperties(updated, Path.GetTempPath(), _loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original", result.CompanyName);
        File.Delete(tempFile);
    }

    [Fact]
    public void GetFilePath_ReturnsCorrectPath()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var logger = new Mock<ILogger<LocalCompanyProfileStore>>();
        var options = Options.Create(new EzLeadSettings
        {
            FileCompanyProfileDirectory = "profiles",
            CacheExpirationInMinutes = 10
        });

        var store = new LocalCompanyProfileStore(cacheService.Object, options, logger.Object);

        var companyId = "acme-corp";

        // Act
        var result = store.GetFilePath(companyId);

        // Assert: Only check the ending segment
        var expectedEnding = Path.Combine("profiles", "companies", $"company.{companyId.FileSystemName()}.json").Replace('\\', '/');
        var actualNormalized = result.Replace('\\', '/');
        Assert.EndsWith(expectedEnding, actualNormalized, StringComparison.OrdinalIgnoreCase);
    }


    private LocalCompanyProfileStore CreateStore(out Mock<ILogger<LocalCompanyProfileStore>> loggerMock)
    {
        var optionsMock = new Mock<IOptions<EzLeadSettings>>();
        optionsMock.Setup(o => o.Value).Returns(new EzLeadSettings
        {
            FileCompanyProfileDirectory = _testDirectory
        });

        loggerMock = new Mock<ILogger<LocalCompanyProfileStore>>();
        return new LocalCompanyProfileStore(_cacheServiceMock.Object, optionsMock.Object, loggerMock.Object);
    }

    private static CompanyProfile CreateProfile(string companyName, DateTime? createdAt = null)
    {
        JobSummary jobSummary = new()
        {
            CompanyName = companyName,
            Division = "Test Division",
        };
        HierarchyResults hierarchyResults = new()
        {
            OrgHierarchy =
             [
                 new HierarchyItem
                 {
                      Name = "Test Department",
                      Title = "test-department",
                 }
             ]
        };
        return new CompanyProfile(jobSummary, hierarchyResults)
        {
            CreatedAt = createdAt ?? DateTime.UtcNow,
        };
    }

}