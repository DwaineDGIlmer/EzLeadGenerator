using Application.Contracts;
using Application.Models;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;
using static WebApp.UnitTests.UnitTestsBase;

namespace WebApp.UnitTests.Pages;

public class CompanyAnalysisTest
{
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
        Assert.Equal("Business", result);
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
    public async Task GetTags_Returns_Empty_When_Tags_Null_Or_Empty()
    {
        // Act
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
        {
            new()
            {
                OrganicResults =
                [
                    new OrganicResult
                    {
                        Title = "Title1",
                        Link = "http://example.com",
                        Source = "Source1",
                        DisplayedLink = "example.com",
                        Date = "2025-08-14",
                        Snippet = "This is a news update.",
                        SnippetHighlightedWords = ["update"],
                    }
                ]
            }
        };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        // Act
        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Equal("News", results.Title);
        Assert.Equal("http://example.com", results.Items[0].Url);
        Assert.Equal("Source1", results.Items[0].Source);
        Assert.Equal("example.com", results.Items[0].DisplayedLink);
        Assert.Equal("2025-08-14", results.Items[0].Date);
        Assert.Equal("This is a news update.", results.Items[0].Snippet);
        Assert.Equal("News", results.Items[0].Type);
        Assert.Contains("update", results.Items[0].Tags);
    }


    [Fact]
    public async Task GetCompanyResults_Returns_Empty_When_OrganicResults_Is_Empty_List()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
        {
            new()
            {
                OrganicResults = []
            }
        };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.Null(results);
    }

    [Fact]
    public async Task GetCompanyResults_Ignores_Null_OrganicResults()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
        {
            new()
            {
                OrganicResults = null!
            }
        };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.Null(results);
    }

    [Fact]
    public async Task GetCompanyResults_Returns_Multiple_ResultItemVm()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
        {
            new()
            {
                OrganicResults =
                [
                    new OrganicResult
                    {
                        Title = "Title1",
                        Link = "http://example.com/1",
                        Source = "Source1",
                        DisplayedLink = "example.com/1",
                        Date = "2025-08-14",
                        Snippet = "This is a news update.",
                        SnippetHighlightedWords = ["update"],
                    },
                    new OrganicResult
                    {
                        Title = "Title2",
                        Link = "http://example.com/2",
                        Source = "Source2",
                        DisplayedLink = "example.com/2",
                        Date = "2025-08-15",
                        Snippet = "This is a program announcement.",
                        SnippetHighlightedWords = ["announcement"],
                    }
                ]
            }
        };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Items.Count);
        Assert.Equal("Title1", results.Items[0].Title);
        Assert.Equal("Title2", results.Items[1].Title);
        Assert.Contains("update", results.Items[0].Tags);
        Assert.Contains("announcement", results.Items[1].Tags);
    }

    [Fact]
    public async Task GetCompanyResults_Handles_Empty_SnippetHighlightedWords()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
        {
            new()
            {
                OrganicResults =
                [
                    new OrganicResult
                    {
                        Title = "Title1",
                        Link = "http://example.com",
                        Source = "Source1",
                        DisplayedLink = "example.com",
                        Date = "2025-08-14",
                        Snippet = "This is a news update.",
                        SnippetHighlightedWords = [],
                    }
                ]
            }
        };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results.Items);
        Assert.Empty(results.Items[0].Tags);
    }


    [Fact]
    public async Task GetCompanyResults_Returns_Correct_Type_For_Program()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
            {
                new()
                {
                    OrganicResults =
                    [
                        new OrganicResult
                        {
                            Title = "Title1",
                            Link = "http://example.com/program",
                            Source = "Source1",
                            DisplayedLink = "example.com/program",
                            Date = "2025-08-14",
                            Snippet = "This is a program update.",
                            SnippetHighlightedWords = ["program"],
                        }
                    ]
                }
            };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results.Items);
        Assert.Equal("News", results.Items[0].Type);
    }

    [Fact]
    public async Task GetCompanyResults_Returns_Correct_Type_For_Jobs()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
            {
                new()
                {
                    OrganicResults =
                    [
                        new OrganicResult
                        {
                            Title = "Title1",
                            Link = "http://example.com/jobs",
                            Source = "Source1",
                            DisplayedLink = "example.com/jobs",
                            Date = "2025-08-14",
                            Snippet = "Find jobs at TestCo.",
                            SnippetHighlightedWords = ["jobs"],
                        }
                    ]
                }
            };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results.Items);
        Assert.Equal("Jobs", results.Items[0].Type);
    }

    [Fact]
    public async Task GetCompanyResults_Returns_Business_When_No_Keywords()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
            {
                new()
                {
                    OrganicResults =
                    [
                        new OrganicResult
                        {
                            Title = "Title1",
                            Link = "http://example.com/business",
                            Source = "Source1",
                            DisplayedLink = "example.com/business",
                            Date = "2025-08-14",
                            Snippet = "General information.",
                            SnippetHighlightedWords = ["information"],
                        }
                    ]
                }
            };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections.FirstOrDefault();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results.Items);
        Assert.Equal("Business", results.Items[0].Type);
    }

    [Fact]
    public async Task GetCompanyResults_Handles_Multiple_Results_With_Mixed_Types()
    {
        // Arrange
        var companyProfile = new CompanyProfile { CompanyName = "TestCo" };
        var organicResults = new List<GoogleSearchResult>
            {
                new()
                {
                    OrganicResults =
                    [
                        new OrganicResult
                        {
                            Title = "Title1",
                            Link = "http://example.com/news",
                            Source = "Source1",
                            DisplayedLink = "example.com/news",
                            Date = "2025-08-14",
                            Snippet = "Latest news update.",
                            SnippetHighlightedWords = ["news"],
                        },
                        new OrganicResult
                        {
                            Title = "Title2",
                            Link = "http://example.com/program",
                            Source = "Source2",
                            DisplayedLink = "example.com/program",
                            Date = "2025-08-15",
                            Snippet = "New program launched.",
                            SnippetHighlightedWords = ["program"],
                        },
                        new OrganicResult
                        {
                            Title = "Title3",
                            Link = "http://example.com/jobs",
                            Source = "Source3",
                            DisplayedLink = "example.com/jobs",
                            Date = "2025-08-16",
                            Snippet = "Job openings available.",
                            SnippetHighlightedWords = ["job"],
                        }
                    ]
                }
            };
        var mockSearch = new MockSearch<List<GoogleSearchResult>>(organicResults);
        var mockDisplayRepository = new Mock<IDisplayRepository>();
        var mockLogger = new Mock<ILogger<CompanyAnalysisPage>>();
        var service = new CompanyAnalysisPage(mockDisplayRepository.Object, mockSearch, mockLogger.Object);

        await service.GetCompanyResults(companyProfile);
        var results = service.Sections;

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal("News", results[0].Items.FirstOrDefault()!.Type);
        Assert.Equal("Program", results[1].Items.FirstOrDefault()!.Type);
        Assert.Equal("Jobs", results[2].Items.FirstOrDefault()!.Type);
    }
}
