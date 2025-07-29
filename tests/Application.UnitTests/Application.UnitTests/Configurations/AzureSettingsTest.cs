using Application.Configurations;
using Application.Constants;

namespace Application.UnitTests.Configurations;

public class AzureSettingsTest
{
    [Fact]
    public void AzureTableName_ShouldHaveDefaultValue()
    {
        var settings = new AzureSettings();
        Assert.Equal(Defaults.AzureTableName, settings.AzureTableName);
    }

    [Fact]
    public void AzureTableName_CanBeSet()
    {
        var settings = new AzureSettings();
        var expected = "CustomTable";
        settings.AzureTableName = expected;
        Assert.Equal(expected, settings.AzureTableName);
    }

    [Fact]
    public void CompanyProfilePartionKey_ShouldHaveDefaultValue()
    {
        var settings = new AzureSettings();
        Assert.Equal(Defaults.CompanyProfilePartionKey, settings.CompanyProfilePartionKey);
    }

    [Fact]
    public void CompanyProfilePartionKey_CanBeSet()
    {
        var settings = new AzureSettings();
        var expected = "CustomCompanyProfileKey";
        settings.CompanyProfilePartionKey = expected;
        Assert.Equal(expected, settings.CompanyProfilePartionKey);
    }

    [Fact]
    public void JobSummaryPartionKey_ShouldHaveDefaultValue()
    {
        var settings = new AzureSettings();
        Assert.Equal(Defaults.JobSummaryPartionKey, settings.JobSummaryPartionKey);
    }

    [Fact]
    public void JobSummaryPartionKey_CanBeSet()
    {
        var settings = new AzureSettings();
        var expected = "CustomJobSummaryKey";
        settings.JobSummaryPartionKey = expected;
        Assert.Equal(expected, settings.JobSummaryPartionKey);
    }

    [Fact]
    public void CompanyProfileTableName_ShouldHaveDefaultValue()
    {
        var settings = new AzureSettings();
        Assert.Equal(Defaults.CompanyProfileTableName, settings.CompanyProfileTableName);
    }

    [Fact]
    public void CompanyProfileTableName_CanBeSet()
    {
        var settings = new AzureSettings();
        var expected = "CustomCompanyProfileTable";
        settings.CompanyProfileTableName = expected;
        Assert.Equal(expected, settings.CompanyProfileTableName);
    }

    [Fact]
    public void JobSummaryTableName_ShouldHaveDefaultValue()
    {
        var settings = new AzureSettings();
        Assert.Equal(Defaults.JobSummaryTableName, settings.JobSummaryTableName);
    }

    [Fact]
    public void JobSummaryTableName_CanBeSet()
    {
        var settings = new AzureSettings();
        var expected = "CustomJobSummaryTable";
        settings.JobSummaryTableName = expected;
        Assert.Equal(expected, settings.JobSummaryTableName);
    }
}
