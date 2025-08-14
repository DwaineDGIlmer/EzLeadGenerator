using Application.Models;

namespace Application.UnitTests.Models;

public class OrganicResultItemTest
{
    [Fact]
    public void DefaultConstructor_InitializesPropertiesWithDefaults()
    {
        var item = new OrganicResultItem();

        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.Url);
        Assert.Equal(string.Empty, item.Location);
        Assert.Null(item.Source);
        Assert.Equal(string.Empty, item.DisplayedLink);
        Assert.Null(item.Date);
        Assert.Equal(string.Empty, item.Snippet);
        Assert.Equal(string.Empty, item.Type);
        Assert.Null(item.MustInclude);
        Assert.NotNull(item.Tags);
        Assert.Empty(item.Tags);
    }

    [Fact]
    public void CanSetAndGetAllProperties()
    {
        var mustInclude = new MustInclude();
        var tags = new[] { "tag1", "tag2" };

        var item = new OrganicResultItem
        {
            Title = "Test Title",
            Url = "http://example.com",
            Location = "Test Location",
            Source = "Test Source",
            DisplayedLink = "example.com",
            Date = "2024-06-01",
            Snippet = "Test Snippet",
            Type = "Test Type",
            MustInclude = mustInclude,
            Tags = tags
        };

        Assert.Equal("Test Title", item.Title);
        Assert.Equal("http://example.com", item.Url);
        Assert.Equal("Test Location", item.Location);
        Assert.Equal("Test Source", item.Source);
        Assert.Equal("example.com", item.DisplayedLink);
        Assert.Equal("2024-06-01", item.Date);
        Assert.Equal("Test Snippet", item.Snippet);
        Assert.Equal("Test Type", item.Type);
        Assert.Equal(mustInclude, item.MustInclude);
        Assert.Equal(tags, item.Tags);
    }
}
