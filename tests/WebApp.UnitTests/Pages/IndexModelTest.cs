using Application.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public sealed class IndexTest
{
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly Mock<IDisplayRepository> _mockDisplayRepository;

    public IndexTest()
    {
        _mockLogger = new Mock<ILogger<IndexModel>>();
        _mockDisplayRepository = new Mock<IDisplayRepository>();
        // Setup mock for GetPaginatedJobs to return an empty list
        _mockDisplayRepository
            .Setup(repo => repo.GetPaginatedJobs(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns([]);
    }

    [Fact]
    public void IndexTest_CanBeConstructed()
    {
        // Arrange & Act
        var pageModel = new IndexModel(_mockDisplayRepository.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(pageModel);
    }
}