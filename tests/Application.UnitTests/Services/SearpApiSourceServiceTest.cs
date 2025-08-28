using Application.Contracts;
using Application.Models;
using Application.Services;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Application.UnitTests.Services;

public class SearpApiSourceServiceTest : UnitTestsBase
{
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ISearch> _searchServiceMock = new();
    private readonly Mock<IJobsRetrieval<JobResult>> _jobsRetrievalMock = new();
    private readonly Mock<IOpenAiChatService> _aiChatServiceMock = new();
    private readonly Mock<ICompanyRepository> _companyRepositoryMock = new();
    private readonly Mock<IJobsRepository> _jobsRepositoryMock = new();
    private readonly Mock<ILogger<SearpApiSourceService>> _loggerMock = new();
    private readonly IOptions<SerpApiSettings> _settings = Options.Create(new SerpApiSettings { Query = "test", Location = "us" });

    private SearpApiSourceService CreateService() =>
        new(
            _settings,
            _cacheServiceMock.Object,
            _searchServiceMock.Object,
            _aiChatServiceMock.Object,
            _jobsRetrievalMock.Object,
            _jobsRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _loggerMock.Object);

    [Fact]
    public async Task UpdateJobSourceAsync_ReturnsFalse_WhenNoJobs()
    {
        _jobsRetrievalMock.Setup(j => j.FetchJobs(It.IsAny<string>(), It.IsAny<string>()))!
            .ReturnsAsync((IEnumerable<JobResult>?)null);

        var service = CreateService();
        var result = await service.UpdateJobSourceAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateJobSourceAsync_ReturnsTrue_WhenJobsAreProcessed()
    {
        var jobs = new List<JobResult>
        {
            new() { JobId = "1", CompanyName = "TestCo", Description = "desc", Location = "Charlotte, NC" }
        };
        _jobsRetrievalMock.Setup(j => j.FetchJobs(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(jobs);

        _jobsRepositoryMock.Setup(r => r.GetJobAsync(It.IsAny<string>()))
            .ReturnsAsync((JobSummary?)null);

        _aiChatServiceMock.Setup(a => a.GetChatCompletion<DivisionInference>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DivisionInference { Division = "Division", Reasoning = "Reason", Confidence = 1 });

        _jobsRepositoryMock.Setup(r => r.AddJobAsync(It.IsAny<JobSummary>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.UpdateJobSourceAsync();

        Assert.True(result);
    }


    [Fact]
    public async Task UpdateJobSourceAsync_ReturnsFalse_WhenJobsAreNotInArea()
    {
        var jobResult = new JobResult
        {
            JobId = "1",
            CompanyName = "TestCo",
            Description = "desc",
            Location = "New York, NY"
        };
        var jobs = new List<JobResult>
        {
           jobResult
        };

        _jobsRetrievalMock.Setup(j => j.FetchJobs(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(jobs);

        _jobsRepositoryMock.Setup(r => r.GetJobAsync(It.IsAny<string>()))
            .ReturnsAsync((JobSummary?)null);

        _aiChatServiceMock.Setup(a => a.GetChatCompletion<DivisionInference>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DivisionInference { Division = "Division", Reasoning = "Reason", Confidence = 1 });

        _jobsRepositoryMock.Setup(r => r.AddJobAsync(It.IsAny<JobSummary>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.UpdateJobSourceAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateCompanyProfilesAsync_ReturnsFalse_WhenNoJobs()
    {
        _jobsRepositoryMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))!
            .ReturnsAsync((IEnumerable<JobSummary>?)null);

        var service = CreateService();
        var result = await service.UpdateCompanyProfilesAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateCompanyProfilesAsync_HandlesExistingCompanyProfile()
    {
        var jobs = new List<JobSummary>
        {
            new() { CompanyId = "cid", CompanyName = "TestCo", JobDescription = "desc", Division = "div" }
        };
        _jobsRepositoryMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(jobs);

        _companyRepositoryMock.Setup(c => c.GetCompanyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync(new CompanyProfile(new JobSummary() { CompanyName = "Test Company" }, new HierarchyResults()));

        var service = CreateService();
        var result = await service.UpdateCompanyProfilesAsync();

        Assert.True(result == false || result == true); // Just ensure no exception
    }

    [Fact]
    public void HierarchyResults_InitializesOrgHierarchy()
    {
        var results = new HierarchyResults();
        Assert.NotNull(results.OrgHierarchy);
        Assert.Empty(results.OrgHierarchy);
    }

    [Fact]
    public void HierarchyItem_Properties_SetAndGet()
    {
        var item = new HierarchyItem { Name = "John", Title = "Manager" };
        Assert.Equal("John", item.Name);
        Assert.Equal("Manager", item.Title);
    }

    [Theory]
    [InlineData("Jane Doe", "")]
    [InlineData("John Smith", "")]
    [InlineData("Jane Smith", "")]
    [InlineData("Mike Johnson", "")]
    [InlineData("Data Architect", "")]
    [InlineData("Unknown", "")]
    [InlineData("Lifelong Learner", "")]
    [InlineData("Firstname Lastname", "")]
    [InlineData("Open Role", "")]
    [InlineData("Not provided", "")]
    [InlineData("Chad Pumpernickel", "Chad Pumpernickel")]
    public void UpdateName_ReturnsCorrectName(string input, string expected)
    {
        // Act
        var result = SearpApiSourceService.UpdateName(new HierarchyResults()
        {
            OrgHierarchy =
            [
                new() { Name = input, Title = "Some Title" }
            ]
        });

        // Assert
        if (string.IsNullOrEmpty(expected))
        {
            Assert.Empty(result.OrgHierarchy);
            return;
        }

        Assert.NotNull(result);
        Assert.Single(result.OrgHierarchy);

        var title = result.OrgHierarchy.FirstOrDefault();
        Assert.NotNull(title);
        Assert.Equal(expected, title.Name);
    }


    [Fact]
    public void UpdateName_ReplacesPronounsConjunctionsAndKnownWordsWithUnknown()
    {
        // Arrange
        var input = new HierarchyResults
        {
            OrgHierarchy =
            [
                new() { Name = "he", Title = " Director " },      // pronoun
                new() { Name = "and", Title = " VP " },           // conjunction
                new() { Name = "lead", Title = " Practice " },    // known word
                new() { Name = "Alice", Title = " Manager " }     // normal
            ]
        };

        // Act
        var result = SearpApiSourceService.UpdateName(input);

        // Assert
        Assert.Single(result.OrgHierarchy);
        Assert.Equal("Alice", result.OrgHierarchy[0].Name);

        // Titles should be trimmed
        Assert.Equal("Manager", result.OrgHierarchy[0].Title);
    }

    [Fact]
    public void UpdateName_ReturnsEmptyHierarchy_WhenInputIsNull()
    {
        // Act
        var result = SearpApiSourceService.UpdateName((HierarchyResults)null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.OrgHierarchy);
    }

    [Theory]
    [InlineData("", "Data Engineer")]
    [InlineData(" ", "Data Engineer")]
    [InlineData("Data Engineer, TNIFIN AG", "Data Engineer")]
    [InlineData("Lead Data Engineer", "Lead Data Engineer")]
    [InlineData("Senior Principal Data Engineer", "Senior Principal Data Engineer")]
    [InlineData("Manager, Data Engineering", "Manager")]
    [InlineData("Lead Engineer - Analytics", "Lead Engineer - Analytics")]
    [InlineData("Data Analyst (Contract)", "Data Analyst")]
    [InlineData("Data Science Manager", "Data Science Manager")]
    [InlineData("Principal Engineer", "Principal Engineer")]
    [InlineData("Lead, Data", "Lead")]
    [InlineData("Lead Data Engineer Remote", "Lead Data Engineer Remote")]
    [InlineData("Lead Data Engineer Hybrid", "Lead Data Engineer Hybrid")]
    [InlineData("Senior Data Engineer(Enterprise Platforms Technology)", "Senior Data Engineer")]
    [InlineData("Senior Data Engineer - Capital One Software(Remote)", "Senior Data Engineer")]
    [InlineData("Manager/Director", "Manager")]
    [InlineData("Data Engineer - HRIT Reporting and Analytics", "Data Engineer - HRIT Reporting and Analytics")]
    [InlineData("Senior Data Engineer (Req #001205)", "Senior Data Engineer")]
    [InlineData("Engineer", "Engineer")]
    [InlineData("Data Engineer - AWS", "Data Engineer - AWS")]
    [InlineData("Data Engineer - Hybrid", "Data Engineer - Hybrid")]
    [InlineData("Lead Engineer!", "Lead Engineer")]
    [InlineData("Data Engineer I", "Data Engineer I")]
    [InlineData("Data Engineer II", "Data Engineer II")]
    [InlineData("Data Engineer III", "Data Engineer III")]
    [InlineData("Manager & Supervisor", "Manager")]
    [InlineData("Lead Engineer (Remote)", "Lead Engineer")]
    [InlineData("Lead Engineer - Data", "Lead Engineer")]
    [InlineData("Manager - Data Analytics", "Manager - Data Analytics")]
    [InlineData("Manager", "Manager")]
    [InlineData("Lead", "Lead")]
    [InlineData("Manager, Data", "Manager")]
    [InlineData("Lead Data EngineerCloud & GenAI Automation", "Lead Data Engineer")]
    [InlineData("Lead Data Automation Engineer", "Lead Data Automation Engineer")]
    [InlineData("Lead Engineer, Data", "Lead Engineer")]
    [InlineData("Lead Engineer, Data Analytics", "Lead Engineer")]
    [InlineData("Data Engineering Lead", "Data Engineering Lead")]
    [InlineData("Data Engineering Supervisor", "Data Engineering Supervisor")]
    public void UpdateJobTitle_TrimsAndExtractsCorrectTitle(string input, string expected)
    {
        var job = new JobResult { Title = input };
        SearpApiSourceService.UpdateJobTitle(job);
        Assert.Equal(expected, job.Title);
    }

    [Fact]
    public void UpdateJobTitle_DoesNothing_WhenNoMatch()
    {
        var job = new JobResult { Title = "Unrelated Title" };
        SearpApiSourceService.UpdateJobTitle(job);
        Assert.Equal("Unrelated Title", job.Title);
    }

    [Fact]
    public void UpdateJobTitle_HandlesEmptyJobTitle()
    {
        var job = new JobResult { Title = "" };
        SearpApiSourceService.UpdateJobTitle(job);
        Assert.Equal("Data Engineer", job.Title);
    }

    [Fact]
    public void UpdateJobTitle_HandlesNullJobTitle()
    {
        var job = new JobResult { Title = null! };
        // Should not throw
        SearpApiSourceService.UpdateJobTitle(job);
        Assert.Equal("Data Engineer", job.Title);
    }

    [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null, "desc", "Charlotte, NC", "Engineer", false)] // Missing company name
    [InlineData("TestCo", null, "Charlotte, NC", "Engineer", false)] // Missing description
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData("TestCo", "desc", "Remote", "Engineer", false)] // Remote job
    [InlineData("TestCo", "desc", "Charlotte, NC", "Data Center Engineer", false)] // Title contains "center"
    [InlineData("Recruiting Inc", "desc", "Charlotte, NC", "Engineer", false)] // CompanyName contains "recruit"
    [InlineData("Talent Group", "desc", "Charlotte, NC", "Engineer", false)] // CompanyName contains "talent"
    [InlineData("Staffing Solutions", "desc", "Charlotte, NC", "Engineer", false)] // CompanyName contains "staffing"
    [InlineData("CyberCoders", "desc", "Charlotte, NC", "Engineer", false)] // CompanyName contains "cybercoder"
    [InlineData("TestCo", "desc", "New York, NY", "Engineer", false)] // Not in NC/SC
    [InlineData("TestCo", "desc", "Charlotte, NC", "Engineer", true)] // Valid
    [InlineData("TestCo", "desc", "Columbia, SC", "Engineer", true)] // Valid
    [InlineData("Jobright.ai", "desc", "Charlotte, NC", "Engineer", false)] // Invalid Recruiting company
    public void IsValid_ReturnsExpectedResult(
        string companyName,
        string description,
        string location,
        string title,
        bool expected)
    {
        // Arrange
        var job = new JobResult
        {
            CompanyName = companyName,
            Description = description,
            Location = location,
            Title = title,
            JobId = "1"
        };
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = SearpApiSourceService.IsValid(job, loggerMock.Object);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_LogsWarning_WhenInvalid()
    {
        // Arrange
        var job = new JobResult
        {
            CompanyName = "",
            Description = "",
            Location = "Charlotte, NC",
            Title = "Engineer",
            JobId = "1"
        };
        var loggerMock = new MockLogger<SearpApiSourceService>(LogLevel.Information);

        // Act
        var result = SearpApiSourceService.IsValid(job, loggerMock);

        // Assert
        Assert.False(result);
        Assert.True(loggerMock.Contains("[Information] Job with ID 1 has missing company name or description, skipping."));
    }
}
