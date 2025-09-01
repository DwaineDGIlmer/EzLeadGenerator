using Application.Models;

namespace Application.UnitTests.Models;

sealed public class JobSummaryTest
{
    [Fact]
    public void DefaultConstructor_InitializesPropertiesWithDefaults()
    {
        var jobSummary = new JobSummary();

        Assert.False(string.IsNullOrEmpty(jobSummary.Id));
        Assert.True((DateTime.Now - jobSummary.PostedDate).TotalSeconds < 5);
        Assert.Equal(string.Empty, jobSummary.JobId);
        Assert.Equal(string.Empty, jobSummary.CompanyId);
        Assert.Equal(string.Empty, jobSummary.CompanyName);
        Assert.Equal(string.Empty, jobSummary.HiringAgency);
        Assert.Equal(string.Empty, jobSummary.JobTitle);
        Assert.Equal(string.Empty, jobSummary.Location);
        Assert.Equal(string.Empty, jobSummary.JobDescription);
        Assert.Equal(string.Empty, jobSummary.Division);
        Assert.Equal(0, jobSummary.Confidence);
        Assert.Equal(string.Empty, jobSummary.Reasoning);
        Assert.Equal(string.Empty, jobSummary.SourceLink);
        Assert.Equal(string.Empty, jobSummary.SourceName);
        Assert.NotNull(jobSummary.JobHighlights);
        Assert.Empty(jobSummary.JobHighlights);
        Assert.True((DateTime.Now - jobSummary.CreatedAt).TotalSeconds < 5);
        Assert.True((DateTime.Now - jobSummary.UpdatedAt).TotalSeconds < 5);
    }

    [Fact]
    public void Constructor_WithJobResult_InitializesPropertiesCorrectly()
    {
        var jobHighlights = new List<JobHighlight> { new() { Title = "Remote" } };
        var jobResult = new JobResult
        {
            CompanyName = "TestCompany",
            Title = "Developer",
            Location = "Remote",
            Description = "Job Description",
            JobHighlights = jobHighlights,
            Via = "Agency",
            ShareLink = "http://example.com",
            JobId = "job-123"
        };

        var jobSummary = new JobSummary(jobResult);

        Assert.Equal("TestCompany", jobSummary.CompanyName);
        Assert.False(string.IsNullOrEmpty(jobSummary.CompanyId));
        Assert.Equal("Developer", jobSummary.JobTitle);
        Assert.Equal("Remote", jobSummary.Location);
        Assert.Equal("Job Description", jobSummary.JobDescription);
        Assert.Equal(jobHighlights, jobSummary.JobHighlights);
        Assert.Equal("Agency", jobSummary.HiringAgency);
        Assert.Equal("http://example.com", jobSummary.SourceLink);
        Assert.Equal("Google Jobs", jobSummary.SourceName);
        Assert.Equal("job-123", jobSummary.JobId);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        var jobSummary = new JobSummary
        {
            JobTitle = "QA Engineer",
            PostedDate = new DateTime(2024, 1, 1)
        };

        var result = jobSummary.ToString();

        Assert.Contains("QA Engineer", result);
    }
}