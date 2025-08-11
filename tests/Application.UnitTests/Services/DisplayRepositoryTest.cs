using Application.Contracts;
using Application.Models;
using Application.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.UnitTests.Services;

public class DisplayRepositoryTest
{
    private readonly Mock<ICompanyRepository> _companyRepoMock;
    private readonly Mock<IJobsRepository> _jobsRepoMock;
    private readonly Mock<ILogger<DisplayRepository>> _loggerMock;

    public DisplayRepositoryTest()
    {
        _companyRepoMock = new Mock<ICompanyRepository>();
        _jobsRepoMock = new Mock<IJobsRepository>();
        _loggerMock = new Mock<ILogger<DisplayRepository>>();
    }

    private DisplayRepository CreateRepository(
        IEnumerable<JobSummary>? jobs = null,
        IEnumerable<CompanyProfile>? companies = null)
    {
        _jobsRepoMock.Setup(r => r.GetJobsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(jobs ?? []);

        var companyList = companies?.ToList() ?? new List<CompanyProfile>();
        _companyRepoMock.Setup(r => r.GetCompanyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => companyList.FirstOrDefault(c => c.Id == id));

        return new DisplayRepository(_companyRepoMock.Object, _jobsRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPaginatedJobsAsync_ReturnsCorrectPage()
    {
        var jobs = Enumerable.Range(1, 10)
            .Select(i => new JobSummary { PostedDate = DateTime.Now.AddDays(-i), Id = i.ToString() })
            .ToList();
        var repo = CreateRepository(jobs);

        // Wait for background loading to complete
        await Task.Delay(100);

        var result = await repo.GetPaginatedJobsAsync(DateTime.Now.AddDays(-15), 1, 5);

        Assert.Equal(5, result.Count());
        Assert.Equal(jobs.OrderByDescending(j => j.PostedDate).Take(5).Select(j => j.Id), result.Select(j => j.Id));
    }

    [Fact]
    public async Task GetPaginatedCompaniesAsync_ReturnsCorrectPage()
    {
        var companies = Enumerable.Range(1, 8)
            .Select(i => new CompanyProfile(new JobSummary { CompanyId = i.ToString() }, new HierarchyResults()) { UpdatedAt = DateTime.Now.AddDays(-i), Id = i.ToString() })
            .ToList();
        var jobs = companies.Select(c => new JobSummary { CompanyId = c.Id, PostedDate = c.UpdatedAt, Id = c.Id }).ToList();
        var repo = CreateRepository(jobs, companies);

        await Task.Delay(100);

        var result = await repo.GetPaginatedCompaniesAsync(DateTime.Now.AddDays(-10), 1, 4);

        Assert.Equal(4, result.Count());
        Assert.Equal(companies.OrderByDescending(c => c.UpdatedAt).Take(4).Select(c => c.Id), result.Select(c => c.Id));
    }

    [Fact]
    public void Constructor_ThrowsIfNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(null!, _jobsRepoMock.Object, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(_companyRepoMock.Object, null!, _loggerMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new DisplayRepository(_companyRepoMock.Object, _jobsRepoMock.Object, null!));
    }
}