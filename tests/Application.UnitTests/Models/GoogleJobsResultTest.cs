using Application.Models;
using System.Text.Json;

namespace Application.UnitTests.Models;

sealed public class GoogleJobsResultTest
{
    [Fact]
    public void GoogleJobsResult_DefaultValues_AreInitialized()
    {
        var result = new GoogleJobsResult();

        Assert.NotNull(result.SearchMetadata);
        Assert.NotNull(result.SearchParameters);
        Assert.NotNull(result.Filters);
        Assert.NotNull(result.JobsResults);
        Assert.NotNull(result.SerpapiPagination);
    }

    [Fact]
    public void JobsSearchMetadata_DefaultValues_AreInitialized()
    {
        var metadata = new JobsSearchMetadata();

        Assert.Equal(string.Empty, metadata.Id);
        Assert.Equal(string.Empty, metadata.Status);
        Assert.Equal(string.Empty, metadata.JsonEndpoint);
        Assert.Equal(string.Empty, metadata.CreatedAt);
        Assert.Equal(string.Empty, metadata.ProcessedAt);
        Assert.Equal(string.Empty, metadata.GoogleJobsUrl);
        Assert.Equal(string.Empty, metadata.RawHtmlFile);
        Assert.Equal(0, metadata.TotalTimeTaken);
    }

    [Fact]
    public void Parameters_DefaultValues_AreInitialized()
    {
        var parameters = new Parameters();

        Assert.Equal(string.Empty, parameters.Uds);
        Assert.Equal(string.Empty, parameters.Q);
    }

    [Fact]
    public void Option_DefaultValues_AreInitialized()
    {
        var option = new Option();

        Assert.Equal(string.Empty, option.Name);
        Assert.NotNull(option.Parameters);
        Assert.Equal(string.Empty, option.Link);
        Assert.Equal(string.Empty, option.SerpapiLink);
    }

    [Fact]
    public void Filter_DefaultValues_AreInitialized()
    {
        var filter = new Filter();

        Assert.Equal(string.Empty, filter.Name);
        Assert.NotNull(filter.Parameters);
        Assert.Equal(string.Empty, filter.Link);
        Assert.Equal(string.Empty, filter.SerpapiLink);
        Assert.NotNull(filter.Options);
    }

    [Fact]
    public void DetectedExtensions_DefaultValues_AreInitialized()
    {
        var ext = new DetectedExtensions();

        Assert.Equal(string.Empty, ext.PostedAt);
        Assert.Equal(string.Empty, ext.ScheduleType);
        Assert.Null(ext.HealthInsurance);
        Assert.Null(ext.DentalCoverage);
        Assert.Null(ext.PaidTimeOff);
    }

    [Fact]
    public void JobHighlight_DefaultValues_AreInitialized()
    {
        var highlight = new JobHighlight();

        Assert.Equal(string.Empty, highlight.Title);
        Assert.NotNull(highlight.Items);
    }

    [Fact]
    public void ApplyOption_DefaultValues_AreInitialized()
    {
        var option = new ApplyOption();

        Assert.Equal(string.Empty, option.Title);
        Assert.Equal(string.Empty, option.Link);
    }

    [Fact]
    public void JobResult_DefaultValues_AreInitialized()
    {
        var job = new JobResult();

        Assert.Equal(string.Empty, job.Title);
        Assert.Equal(string.Empty, job.CompanyName);
        Assert.Equal(string.Empty, job.Location);
        Assert.Equal(string.Empty, job.Via);
        Assert.Equal(string.Empty, job.ShareLink);
        Assert.NotNull(job.Extensions);
        Assert.NotNull(job.DetectedExtensions);
        Assert.Equal(string.Empty, job.Description);
        Assert.NotNull(job.JobHighlights);
        Assert.NotNull(job.ApplyOptions);
        Assert.Equal(string.Empty, job.JobId);
    }

    [Fact]
    public void SerpapiPagination_DefaultValues_AreInitialized()
    {
        var pagination = new SerpapiPagination();

        Assert.Equal(string.Empty, pagination.NextPageToken);
        Assert.Equal(string.Empty, pagination.Next);
    }

    [Fact]
    public void GoogleJobsResult_CanBeSerializedAndDeserialized()
    {
        var result = new GoogleJobsResult
        {
            SearchMetadata = new JobsSearchMetadata { Id = "123", Status = "done" },
            SearchParameters = new SearchParameters(),
            Filters = [new Filter { Name = "Location" }],
            JobsResults = [new JobResult { Title = "Developer" }],
            SerpapiPagination = new SerpapiPagination { NextPageToken = "token" }
        };

        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<GoogleJobsResult>(json);

        Assert.Equal("123", deserialized!.SearchMetadata.Id);
        Assert.Equal("done", deserialized.SearchMetadata.Status);
        Assert.Single(deserialized.Filters);
        Assert.Single(deserialized.JobsResults);
        Assert.Equal("Developer", deserialized.JobsResults[0].Title);
        Assert.Equal("token", deserialized.SerpapiPagination.NextPageToken);
    }
}