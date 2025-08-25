using Application.Configurations;
using Application.Constants;

namespace Application.UnitTests.Configurations;

public class EzLeadSettingsTest
{
    [Fact]
    public void Default_JobExecutionInSeconds_ShouldBe3600()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(3600, settings.JobExecutionInSeconds);
    }

    [Fact]
    public void Default_FileCompanyProfileDirectory_ShouldBeEmpty()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(string.Empty, settings.FileCompanyProfileDirectory);
    }

    [Fact]
    public void Default_FileJobProfileDirectory_ShouldBeEmpty()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(string.Empty, settings.FileJobProfileDirectory);
    }

    [Fact]
    public void Default_CompanyCacheExpirationInDays_ShouldMatchConstant()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(Defaults.CompanyCacheExpirationInDays, settings.CompanyCacheExpirationInDays);
    }

    [Fact]
    public void Default_JobsCacheExpirationInHours_ShouldMatchConstant()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(Defaults.JobsCacheExpirationInHours, settings.JobsCacheExpirationInHours);
    }

    [Fact]
    public void Default_SerpApiQueryExpirationInMinutes_ShouldMatchConstant()
    {
        var settings = new EzLeadSettings();
        Assert.Equal(Defaults.SerpApiQueryExpirationInMinutes, settings.SerpApiQueryExpirationInMinutes);
    }

    [Fact]
    public void Can_Set_Properties()
    {
        var settings = new EzLeadSettings
        {
            JobExecutionInSeconds = 1800,
            FileCompanyProfileDirectory = "company_dir",
            FileJobProfileDirectory = "job_dir",
            CompanyCacheExpirationInDays = 10,
            JobsCacheExpirationInHours = 5,
            SerpApiQueryExpirationInMinutes = 15
        };

        Assert.Equal(1800, settings.JobExecutionInSeconds);
        Assert.Equal("company_dir", settings.FileCompanyProfileDirectory);
        Assert.Equal("job_dir", settings.FileJobProfileDirectory);
        Assert.Equal(10, settings.CompanyCacheExpirationInDays);
        Assert.Equal(5, settings.JobsCacheExpirationInHours);
        Assert.Equal(15, settings.SerpApiQueryExpirationInMinutes);
    }
}