using Application.Configurations;
using Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Middleware;

namespace WebApp.UnitTests.Middleware;

public sealed class JobServicesMiddlewareTest : UnitTestsBase
{
    private readonly Mock<IJobSourceService> _jobSourceServiceMock = new();
    private readonly Mock<ILogger<JobServicesMiddleware>> _loggerMock = new();
    private readonly Mock<RequestDelegate> _nextMock = new();

    private static IOptions<EzLeadSettings> CreateOptions(int jobExecutionInSeconds = 28800)
    {
        return Options.Create(new EzLeadSettings { JobExecutionInSeconds = jobExecutionInSeconds });
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobServicesMiddleware(
                null!,
                CreateOptions(),
                _jobSourceServiceMock.Object,
                _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobServicesMiddleware(
                _nextMock.Object,
                null!,
                _jobSourceServiceMock.Object,
                _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenJobSourceServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobServicesMiddleware(
                _nextMock.Object,
                CreateOptions(),
                null!,
                _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobServicesMiddleware(
                _nextMock.Object,
                CreateOptions(),
                _jobSourceServiceMock.Object,
                null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenJobExecutionInSecondsIsNotSet()
    {
        var options = Options.Create(new EzLeadSettings { JobExecutionInSeconds = 0 });
        Assert.Throws<ArgumentNullException>(() =>
            new JobServicesMiddleware(
                _nextMock.Object,
                options,
                _jobSourceServiceMock.Object,
                _loggerMock.Object));
    }

    [Fact]
    public async Task InvokeAsync_UpdatesJobSources_WhenLastExecutionIsDefault()
    {
        // Arrange  
        JobServicesMiddleware.LastExecution = default;
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(1),
            _jobSourceServiceMock.Object,
            _loggerMock.Object);

        var context = new DefaultHttpContext();

        _jobSourceServiceMock.Setup(s => s.UpdateCompanyProfilesAsync()).Returns(Task.FromResult(true)).Verifiable();
        _jobSourceServiceMock.Setup(s => s.UpdateJobSourceAsync()).Returns(Task.FromResult(true)).Verifiable();
        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask).Verifiable();

        // Act  
        await middleware.InvokeAsync(context);

        // Assert  
        _jobSourceServiceMock.Verify(s => s.UpdateCompanyProfilesAsync(), Times.Once);
        _jobSourceServiceMock.Verify(s => s.UpdateJobSourceAsync(), Times.Once);
        _nextMock.Verify(n => n(context), Times.Once);
        Assert.NotEqual(default, JobServicesMiddleware.LastExecution);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotUpdateJobSources_WhenIntervalNotElapsed()
    {
        // Arrange  
        JobServicesMiddleware.LastExecution = DateTime.UtcNow;
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(3600),
            _jobSourceServiceMock.Object,
            _loggerMock.Object);

        var context = new DefaultHttpContext();

        _jobSourceServiceMock.Setup(s => s.UpdateCompanyProfilesAsync()).Returns(Task.FromResult(true));
        _jobSourceServiceMock.Setup(s => s.UpdateJobSourceAsync()).Returns(Task.FromResult(true));
        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask).Verifiable();

        // Act  
        await middleware.InvokeAsync(context);

        // Assert  
        _jobSourceServiceMock.Verify(s => s.UpdateCompanyProfilesAsync(), Times.Never);
        _jobSourceServiceMock.Verify(s => s.UpdateJobSourceAsync(), Times.Never);
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_UpdatesJobSources_WhenIntervalElapsed()
    {
        // Arrange  
        JobServicesMiddleware.LastExecution = DateTime.UtcNow.AddHours(-9);
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(28800), // 8 hours  
            _jobSourceServiceMock.Object,
            _loggerMock.Object);

        var context = new DefaultHttpContext();

        _jobSourceServiceMock.Setup(s => s.UpdateCompanyProfilesAsync()).Returns(Task.FromResult(true)).Verifiable();
        _jobSourceServiceMock.Setup(s => s.UpdateJobSourceAsync()).Returns(Task.FromResult(true)).Verifiable();
        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask).Verifiable();

        // Act  
        await middleware.InvokeAsync(context);

        // Wait for the background task to complete
        await Task.Delay(100); // Adjust delay as needed for your environment

        // Assert  
        _jobSourceServiceMock.Verify(s => s.UpdateCompanyProfilesAsync(), Times.Once);
        _jobSourceServiceMock.Verify(s => s.UpdateJobSourceAsync(), Times.Once);
        _nextMock.Verify(n => n(context), Times.Once);
        Assert.True(JobServicesMiddleware.LastExecution > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task InvokeAsync_LogsErrorAndReturns_WhenHttpContextIsNull()
    {
        // Arrange
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(10),
            _jobSourceServiceMock.Object,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(null!);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HttpContext is null in JobServicesMiddleware.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_CallsUpdateSourceAsync_WhenLastExecutionIsDefault()
    {
        // Arrange
        JobServicesMiddleware.LastExecution = default;
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(10),
            _jobSourceServiceMock.Object,
            _loggerMock.Object);

        var context = new DefaultHttpContext();

        _jobSourceServiceMock.Setup(s => s.UpdateCompanyProfilesAsync()).Returns(Task.FromResult(true)).Verifiable();
        _jobSourceServiceMock.Setup(s => s.UpdateJobSourceAsync()).Returns(Task.FromResult(true)).Verifiable();
        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask).Verifiable();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _jobSourceServiceMock.Verify(s => s.UpdateCompanyProfilesAsync(), Times.Once);
        _jobSourceServiceMock.Verify(s => s.UpdateJobSourceAsync(), Times.Once);
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsUserInformation_WhenUserIsAnonymousOrAuthenticated()
    {
        // Arrange
        JobServicesMiddleware.LastExecution = default;
        var mockLogger = new MockLogger<JobServicesMiddleware>(LogLevel.Information);
        var middleware = new JobServicesMiddleware(
            _nextMock.Object,
            CreateOptions(10),
            _jobSourceServiceMock.Object,
            mockLogger);

        // Anonymous user
        var anonymousContext = new DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity())
        };

        // Authenticated user
        var identity = new System.Security.Claims.ClaimsIdentity("TestAuthType");
        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser"));
        identity = new System.Security.Claims.ClaimsIdentity(identity.Claims, "TestAuthType", identity.NameClaimType, identity.RoleClaimType);
        var authenticatedContext = new DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(identity)
        };

        // Act
        await middleware.InvokeAsync(anonymousContext);
        await middleware.InvokeAsync(authenticatedContext);

        // Assert
        Assert.True(mockLogger.Contains("[Warning] User logged in: Anonymous"));
        Assert.True(mockLogger.Contains("[Warning] User logged in: TestUser"));
    }
}