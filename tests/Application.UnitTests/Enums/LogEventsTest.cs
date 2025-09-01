using Application.Enums;

namespace Application.UnitTests.Enums;

public sealed class LogEventsTest
{
    [Theory]
    [InlineData(LogEvents.None, 0)]
    [InlineData(LogEvents.Exception, 1001)]
    [InlineData(LogEvents.Warning, 1002)]
    [InlineData(LogEvents.Information, 1003)]
    [InlineData(LogEvents.CacheHit, 2001)]
    [InlineData(LogEvents.CacheMiss, 2002)]
    [InlineData(LogEvents.UserLogin, 3001)]
    [InlineData(LogEvents.UserLogout, 3002)]
    public void LogEvents_HasExpectedValues(LogEvents logEvent, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)logEvent);
    }

    [Fact]
    public void LogEvents_EnumContainsAllExpectedNames()
    {
        var names = Enum.GetNames(typeof(LogEvents));
        Assert.Contains("None", names);
        Assert.Contains("Exception", names);
        Assert.Contains("Warning", names);
        Assert.Contains("Information", names);
        Assert.Contains("CacheHit", names);
        Assert.Contains("CacheMiss", names);
        Assert.Contains("UserLogin", names);
        Assert.Contains("UserLogout", names);
        Assert.Equal(8, names.Length);
    }
}