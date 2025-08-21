using Application.Models;

namespace Application.UnitTests.Models;

public class OrganicResultTest
{
    [Fact]
    public void OrganicResult_DefaultValues_AreSetCorrectly()
    {
        var result = new OrganicResult();

        Assert.Equal(0, result.Position);
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Link);
        Assert.Equal(string.Empty, result.RedirectLink);
        Assert.Equal(string.Empty, result.DisplayedLink);
        Assert.Equal(string.Empty, result.Thumbnail);
        Assert.Equal(string.Empty, result.Date);
        Assert.Equal(string.Empty, result.Snippet);
        Assert.NotNull(result.SnippetHighlightedWords);
        Assert.Empty(result.SnippetHighlightedWords);
        Assert.NotNull(result.RichSnippet);
        Assert.NotNull(result.RichSnippet.Top);
        Assert.NotNull(result.RichSnippet.Top.Extensions);
        Assert.Empty(result.RichSnippet.Top.Extensions);
        Assert.Null(result.MustInclude);
        Assert.Equal(string.Empty, result.Source);
    }

    [Fact]
    public void OrganicResult_SetProperties_ValuesAreAssigned()
    {
        var result = new OrganicResult
        {
            Position = 1,
            Title = "Test Title",
            Link = "http://example.com",
            RedirectLink = "http://redirect.com",
            DisplayedLink = "example.com",
            Thumbnail = "http://image.com/thumb.jpg",
            Date = "2024-06-01",
            Snippet = "This is a snippet.",
            SnippetHighlightedWords = ["snippet", "test"],
            RichSnippet = new TopInfo
            {
                Top = new TopDetails
                {
                    Extensions = [".pdf", ".docx"]
                }
            },
            MustInclude = new MustInclude
            {
                Word = "important",
                Link = "http://word.com"
            },
            Source = "Google"
        };

        Assert.Equal(1, result.Position);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("http://example.com", result.Link);
        Assert.Equal("http://redirect.com", result.RedirectLink);
        Assert.Equal("example.com", result.DisplayedLink);
        Assert.Equal("http://image.com/thumb.jpg", result.Thumbnail);
        Assert.Equal("2024-06-01", result.Date);
        Assert.Equal("This is a snippet.", result.Snippet);
        Assert.Equal(["snippet", "test"], result.SnippetHighlightedWords);
        Assert.NotNull(result.RichSnippet);
        Assert.NotNull(result.RichSnippet.Top);
        Assert.Equal([".pdf", ".docx"], result.RichSnippet.Top.Extensions);
        Assert.NotNull(result.MustInclude);
        Assert.Equal("important", result.MustInclude.Word);
        Assert.Equal("http://word.com", result.MustInclude.Link);
        Assert.Equal("Google", result.Source);
    }

    [Fact]
    public void MustInclude_DefaultValues_AreSetCorrectly()
    {
        var mustInclude = new MustInclude();
        Assert.Equal(string.Empty, mustInclude.Word);
        Assert.Equal(string.Empty, mustInclude.Link);
    }

    [Fact]
    public void TopInfo_DefaultValues_AreSetCorrectly()
    {
        var info = new TopInfo();
        Assert.NotNull(info.Top);
        Assert.NotNull(info.Top.Extensions);
        Assert.Empty(info.Top.Extensions);
    }

    [Fact]
    public void TopDetails_DefaultValues_AreSetCorrectly()
    {
        var details = new TopDetails();
        Assert.NotNull(details.Extensions);
        Assert.Empty(details.Extensions);
    }
}