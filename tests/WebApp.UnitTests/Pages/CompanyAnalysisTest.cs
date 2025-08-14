using Application.Contracts;
using Application.Models;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public class CompanyAnalysisTest
{
    [Fact]
    public void CompanyAnalysisTest_CanBeConstructed()
    {
        // Arrange & Act
        var pageModel = new CompanyAnalysisTest();

        // Assert
        Assert.NotNull(pageModel);
    }
    [Fact]
    public void GetTitle_Returns_News_When_NewsKeyword_In_Snippet()
    {
        // Arrange
        string snippet = "This is a news update about the company.";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTitle(snippet, displayLink);

        // Assert
        Assert.Equal("News", result);
    }

    [Fact]
    public void GetTitle_Returns_Program_When_ProgramKeyword_In_DisplayLink()
    {
        // Arrange
        string snippet = "Some random text";
        string displayLink = "example.com/program";

        // Act
        var result = CompanyAnalysisPage.GetTitle(snippet, displayLink);

        // Assert
        Assert.Equal("Program", result);
    }

    [Fact]
    public void GetTitle_Returns_Job_When_JobKeyword_In_Snippet()
    {
        // Arrange
        string snippet = "Looking for a job opportunity";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTitle(snippet, displayLink);

        // Assert
        Assert.Equal("Jobs", result);
    }

    [Fact]
    public void GetTitle_Returns_Business_When_No_Keywords()
    {
        // Arrange
        string snippet = "No keywords here";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTitle(snippet, displayLink);

        // Assert
        Assert.Equal("business", result);
    }

    [Fact]
    public void GetTags_Filters_CompanyName_And_Adds_NewsUpdate()
    {
        // Arrange
        string companyName = "TestCo";
        var tags = new List<string> { "TestCo", "Tag1" };
        string snippet = "This is a news story";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTags(companyName, tags, snippet, displayLink);

        // Assert
        Assert.Contains("Tag1", result);
        Assert.Contains("recent", result);
        Assert.DoesNotContain("TestCo", result);
    }

    [Fact]
    public void GetTags_Adds_Program_And_Jobs()
    {
        // Arrange
        string companyName = "TestCo";
        var tags = new List<string> { "Tag2" };
        string snippet = "This program offers many jobs";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTags(companyName, tags, snippet, displayLink);

        // Assert
        Assert.Contains("initiative", result);
        Assert.Contains("careers", result);
    }

    [Fact]
    public void GetTags_Returns_Empty_When_Tags_Null_Or_Empty()
    {
        // Arrange
        string companyName = "TestCo";
        List<string> tags = null!;
        string snippet = "Some snippet";
        string displayLink = "example.com";

        // Act
        var result = CompanyAnalysisPage.GetTags(companyName, tags, snippet, displayLink);

        // Assert
        Assert.Empty(result);

        // Arrange
        tags = new List<string>();
        result = CompanyAnalysisPage.GetTags(companyName, tags, snippet, displayLink);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetCompanyResults_Returns_Empty_When_No_Results()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var mockSearch = new Mock<ISearch<OrganicResult>>();
        mockSearch.Setup(s => s.FetchOrganicResults(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<OrganicResult>)null!);

        // Act
        var results = CompanyAnalysisPage.GetCompanyResults(companyProfile, mockSearch.Object);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void GetCompanyResults_Returns_ResultItemVm_List()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<OrganicResult>
        {
            new() {
                Title = "Title1",
                Link = "http://example.com",
                Source = "Source1",
                DisplayedLink = "example.com",
                Date = "2025-08-14",
                Snippet = "This is a news update.",
                SnippetHighlightedWords = ["update"],
            }
        };
        var mockSearch = new Mock<ISearch<OrganicResult>>();
        mockSearch.Setup(s => s.FetchOrganicResults(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(organicResults);

        // Act
        var results = CompanyAnalysisPage.GetCompanyResults(companyProfile, mockSearch.Object);

        // Assert
        Assert.Single(results);
        Assert.Equal("Title1", results[0].Title);
        Assert.Equal("http://example.com", results[0].Url);
        Assert.Equal("Source1", results[0].Source);
        Assert.Equal("example.com", results[0].DisplayedLink);
        Assert.Equal("2025-08-14", results[0].Date);
        Assert.Equal("This is a news update.", results[0].Snippet);
        Assert.Equal("News", results[0].Type);
        Assert.Contains("update", results[0].Tags);
    }
}
