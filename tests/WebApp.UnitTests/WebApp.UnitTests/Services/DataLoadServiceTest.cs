using Application.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApp.Services;

namespace WebApp.UnitTests.Services;

public class DataLoadServiceTest
{
    [Fact]
    public async Task LoadAppSourceService_UpdatesJobSourceAndCompanyProfiles()
    {
        // Arrange
        var jobSourceServiceMock = new Mock<IJobSourceService>();
        jobSourceServiceMock.Setup(s => s.UpdateJobSourceAsync()).Returns(Task.FromResult(true)).Verifiable();
        jobSourceServiceMock.Setup(s => s.UpdateCompanyProfilesAsync()).Returns(Task.FromResult(true)).Verifiable();

        var serviceProvider = new ServiceCollection()
            .AddSingleton(jobSourceServiceMock.Object)
            .BuildServiceProvider();

        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProvider);

        // Act
        await DataLoadService.LoadAppSourceService(appBuilderMock.Object);

        // Assert
        jobSourceServiceMock.Verify(s => s.UpdateJobSourceAsync(), Times.Once);
        jobSourceServiceMock.Verify(s => s.UpdateCompanyProfilesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoadAppSourceService_ThrowsIfJobSourceServiceNotRegistered()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await DataLoadService.LoadAppSourceService(appBuilderMock.Object);
        });
    }
}