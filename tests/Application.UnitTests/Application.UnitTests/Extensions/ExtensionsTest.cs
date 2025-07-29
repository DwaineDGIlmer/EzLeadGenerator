using Application.Extensions;

namespace Application.UnitTests.Extensions;

public class ExtensionsTest
{
    [Fact]
    public void FileSystemName_ReturnsEmpty_WhenInputIsNull()
    {
        string input = null!;
        var result = input.FileSystemName();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FileSystemName_ReturnsEmpty_WhenInputIsEmpty()
    {
        string input = "";
        var result = input.FileSystemName();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FileSystemName_ReturnsSanitizedHash_WithDefaultLength()
    {
        string input = "TestFileName";
        var result = input.FileSystemName();
        Assert.False(string.IsNullOrEmpty(result));
        Assert.True(result.Length <= 64);
    }

    [Fact]
    public void FileSystemName_ReturnsSanitizedHash_WithCustomLength()
    {
        string input = "AnotherTestFileName";
        int customLength = 10;
        var result = input.FileSystemName(customLength);
        Assert.False(string.IsNullOrEmpty(result));
        Assert.True(result.Length <= customLength);
    }
}