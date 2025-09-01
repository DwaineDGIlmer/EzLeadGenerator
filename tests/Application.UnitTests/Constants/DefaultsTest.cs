using Application.Constants;

namespace Application.UnitTests.Constants;

public sealed class DefaultsTest
{
    [Fact]
    public void SerpApiQueryExpirationInMinutes_ShouldBeCorrect()
    {
        Assert.Equal(1440, Defaults.SerpApiQueryExpirationInMinutes);
    }

    [Fact]
    public void JobsCacheExpirationInHours_ShouldBeCorrect()
    {
        Assert.Equal(8, Defaults.JobsCacheExpirationInHours);
    }

    [Fact]
    public void CompanyCacheExpirationInDays_ShouldBeCorrect()
    {
        Assert.Equal(5, Defaults.CompanyCacheExpirationInDays);
    }

    [Fact]
    public void JobSummaryPartionKey_ShouldBeCorrect()
    {
        Assert.Equal("jobsummary", Defaults.JobSummaryPartionKey);
    }

    [Fact]
    public void CompanyProfilePartionKey_ShouldBeCorrect()
    {
        Assert.Equal("companyprofile", Defaults.CompanyProfilePartionKey);
    }

    [Fact]
    public void EnvSearchApiUrl_ShouldBeCorrect()
    {
        Assert.Equal("SEARCH_SERPAPI_API_URL", Defaults.EnvSearchApiUrl);
    }

    [Fact]
    public void EnvSearchApiKey_ShouldBeCorrect()
    {
        Assert.Equal("SEARCH_SERPAPI_API_KEY", Defaults.EnvSearchApiKey);
    }

    [Fact]
    public void LeadFileName_ShouldBeCorrect()
    {
        Assert.Equal("leads.csv", Defaults.LeadFileName);
    }

    [Fact]
    public void CsvMimeType_ShouldBeCorrect()
    {
        Assert.Equal("text/csv", Defaults.CsvMimeType);
    }

    [Fact]
    public void JsonMimeType_ShouldBeCorrect()
    {
        Assert.Equal("application/json", Defaults.JsonMimeType);
    }

    [Fact]
    public void EzLeadsSettings_ShouldBeCorrect()
    {
        Assert.Equal("EzLeadsSettings", Defaults.EzLeadsSettings);
    }

    [Fact]
    public void SearchApiKey_ShouldBeCorrect()
    {
        Assert.Equal("SearchApiKey", Defaults.SearchApiKey);
    }

    [Fact]
    public void SearchEndpoint_ShouldBeCorrect()
    {
        Assert.Equal("SearchEndpoint", Defaults.SearchEndpoint);
    }

    [Theory]
    [InlineData("EzLeadsSettings")]
    [InlineData("SearchApiKey")]
    [InlineData("SearchEndpoint")]
    public void GetSettings_ValidKeys_ReturnsFormattedString(string key)
    {
        var expected = $"{Defaults.EzLeadsSettings}:{key}";
        Assert.Equal(expected, Defaults.GetSettings(key));
    }

    [Theory]
    [InlineData("InvalidKey")]
    [InlineData("")]
    public void GetSettings_InvalidKey_ThrowsArgumentException(string key)
    {
        Assert.Throws<ArgumentException>(() => Defaults.GetSettings(key));
    }
}