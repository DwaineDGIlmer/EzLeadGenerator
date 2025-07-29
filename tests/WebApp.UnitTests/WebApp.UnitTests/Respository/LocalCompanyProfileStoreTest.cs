using Application.Models;
using Application.Services;
using Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Respository;

namespace WebApp.UnitTests.Respository;

public class LocalCompanyProfileStoreTest
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    private LocalCompanyProfileStore CreateStore(out Mock<ILogger<LocalCompanyProfileStore>> loggerMock)
    {
        var optionsMock = new Mock<IOptions<SerpApiSettings>>();
        optionsMock.Setup(o => o.Value).Returns(new SerpApiSettings
        {
            FileCompanyProfileDirectory = _testDirectory
        });

        loggerMock = new Mock<ILogger<LocalCompanyProfileStore>>();
        return new LocalCompanyProfileStore(optionsMock.Object, loggerMock.Object);
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

    [Fact]
    public async Task GetCompanyProfileAsync_ReturnsProfile_WhenExists()
    {
        var store = CreateStore(out var _);
        var profile = CreateProfile("get-company");
        await store.AddCompanyProfileAsync(profile);

        var result = await store.GetCompanyProfileAsync(profile.CompanyName);

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

        var updated = await store.GetCompanyProfileAsync(profile.CompanyName);
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

    // Cleanup test directory after each test
    public LocalCompanyProfileStoreTest()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
        Directory.CreateDirectory(_testDirectory);
    }
}