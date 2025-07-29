using Application.Extensions;
using Application.Models;
using Application.Services;

namespace Application.UnitTests.Models;

public class CompanyProfileTest
{
    [Fact]
    public void DefaultConstructor_InitializesProperties()
    {
        var profile = new CompanyProfile(new JobSummary(), new HierarchyResults());

        Assert.False(string.IsNullOrEmpty(profile.Id));
        Assert.Equal(string.Empty, profile.CompanyId);
        Assert.Equal(string.Empty, profile.CompanyName);
        Assert.NotNull(profile.HierarchyResults);
        Assert.True(profile.CreatedAt <= DateTime.UtcNow);
        Assert.True(profile.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void ParameterizedConstructor_SetsPropertiesCorrectly()
    {
        var jobSummary = new JobSummary { CompanyName = "TestCompany" };
        var hierarchyResults = new HierarchyResults();

        var profile = new CompanyProfile(jobSummary, hierarchyResults);

        Assert.Equal("TestCompany", profile.CompanyName);
        Assert.Equal(jobSummary.CompanyName.FileSystemName(), profile.CompanyId);
        Assert.Equal(hierarchyResults, profile.HierarchyResults);
    }

    [Fact]
    public void ParameterizedConstructor_NullHierarchyResults_CreatesNewInstance()
    {
        var jobSummary = new JobSummary { CompanyName = "TestCompany" };

        var profile = new CompanyProfile(jobSummary, null!);

        Assert.NotNull(profile.HierarchyResults);
    }

    [Fact]
    public void UpdatedAt_Setter_UpdatesValue()
    {
        var profile = new CompanyProfile(new JobSummary(), new HierarchyResults());
        var newDate = DateTime.UtcNow.AddDays(1);

        profile.UpdatedAt = newDate;

        Assert.Equal(newDate, profile.UpdatedAt);
    }
}