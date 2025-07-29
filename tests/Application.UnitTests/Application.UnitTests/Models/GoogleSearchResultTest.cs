using System.Text.Json;

namespace Application.Models.Tests;

public class GoogleSearchResultTest
{
    [Fact]
    public void GoogleSearchResult_DefaultProperties_AreInitialized()
    {
        var result = new GoogleSearchResult();

        Assert.NotNull(result.SearchMetadata);
        Assert.NotNull(result.SearchParameters);
        Assert.NotNull(result.SearchInformation);
        Assert.NotNull(result.OrganicResults);
        Assert.NotNull(result.RelatedSearches);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.SerpApiPagination);
    }

    [Fact]
    public void OrganicResult_DefaultProperties_AreInitialized()
    {
        var organic = new OrganicResult();

        Assert.Equal(0, organic.Position);
        Assert.Equal(string.Empty, organic.Title);
        Assert.Equal(string.Empty, organic.Link);
        Assert.Equal(string.Empty, organic.DisplayedLink);
        Assert.Equal(string.Empty, organic.Thumbnail);
        Assert.Equal(string.Empty, organic.Date);
        Assert.Equal(string.Empty, organic.Snippet);
        Assert.NotNull(organic.SnippetHighlightedWords);
    }

    [Fact]
    public void RelatedSearch_DefaultProperties_AreInitialized()
    {
        var related = new RelatedSearch();

        Assert.Equal(string.Empty, related.Query);
        Assert.Equal(string.Empty, related.Link);
    }

    [Fact]
    public void Pagination_DefaultProperties_AreInitialized()
    {
        var pagination = new Pagination();

        Assert.Equal(0, pagination.Current);
        Assert.Equal(string.Empty, pagination.Previous);
        Assert.Equal(string.Empty, pagination.Next);
        Assert.NotNull(pagination.OtherPages);
    }

    [Fact]
    public void SerpApiPagination_DefaultProperties_AreInitialized()
    {
        var serpApiPagination = new SerpApiPagination();

        Assert.Equal(0, serpApiPagination.Current);
        Assert.Equal(string.Empty, serpApiPagination.PreviousLink);
        Assert.Equal(string.Empty, serpApiPagination.NextLink);
        Assert.NotNull(serpApiPagination.OtherPages);
    }

    [Fact]
    public void GoogleSearchResult_CanBeSerializedAndDeserialized()
    {
        var original = new GoogleSearchResult
        {
            SearchMetadata = new SearchMetadata { Id = "test-id", Status = "done" },
            SearchParameters = new SearchParameters { Engine = "google", Query = "test" },
            SearchInformation = new SearchInformation { OrganicResultsState = "complete", QueryDisplayed = "test", TotalResults = 42, PageNumber = 1, TimeTakenDisplayed = 0.5 },
            OrganicResults = [new OrganicResult { Position = 1, Title = "Title", Link = "http://example.com" }],
            RelatedSearches = [new RelatedSearch { Query = "related", Link = "http://related.com" }],
            Pagination = new Pagination { Current = 1, Previous = "prev", Next = "next", OtherPages = { { "2", "page2" } } },
            SerpApiPagination = new SerpApiPagination { Current = 1, PreviousLink = "prev", NextLink = "next", OtherPages = { { "2", "page2" } } }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GoogleSearchResult>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("test-id", deserialized!.SearchMetadata.Id);
        Assert.Equal("google", deserialized.SearchParameters.Engine);
        Assert.Equal("complete", deserialized.SearchInformation.OrganicResultsState);
        Assert.Single(deserialized.OrganicResults);
        Assert.Single(deserialized.RelatedSearches);
        Assert.Equal(1, deserialized.Pagination.Current);
        Assert.Equal(1, deserialized.SerpApiPagination.Current);
    }
}