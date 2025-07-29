using Application.Constants;

namespace Application.UnitTests.Constants;

public class DefaultsTest
{
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
    [InlineData(null)]
    public void GetSettings_InvalidKey_ThrowsArgumentException(string key)
    {
        Assert.Throws<ArgumentException>(() => Defaults.GetSettings(key));
    }
}