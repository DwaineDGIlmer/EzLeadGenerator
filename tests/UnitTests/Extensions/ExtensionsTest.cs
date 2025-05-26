using WebApp.Extensions;

namespace UnitTests.Extensions;

public class ExtensionsTest
{
    [Fact]
    public void IsNull_ShouldReturnTrueIfObjectIsNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = obj.IsNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNull_ShouldReturnFalseIfObjectIsNotNull()
    {
        // Arrange
        var obj = new object();

        // Act
        var result = obj.IsNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueIfStringIsNull()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueIfStringIsEmpty()
    {
        // Arrange
        string str = "";

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseIfStringIsNotEmpty()
    {
        // Arrange
        string str = "Test";

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNull_ShouldReturnTrueIfObjectIsNotNull()
    {
        // Arrange
        var obj = new object();

        // Act
        var result = obj.IsNotNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotNull_ShouldReturnFalseIfObjectIsNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = obj.IsNotNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullThrow_WithStringValue_Success()
    {
        string value = "Testing!";
        var results = value.IsNullThrow();
        Assert.Equal(value, results);
    }

    [Fact]
    public void IsNullThrow_WithNullString_ThrowsArgumentNullException()
    {
        string value = null!;
        Assert.Throws<ArgumentNullException>(() => value.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithEmptyString_ThrowsArgumentNullException()
    {
        string value = "";
        Assert.Throws<ArgumentNullException>(() => value.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithNonEmptyString_DoesNotThrow()
    {
        string value = "test";
        value.IsNullThrow(); // Should not throw
    }

    [Fact]
    public void IsNullThrow_WithNonStringNullObject_Doeshrow()
    {
        object value = null!;
        Assert.Throws<ArgumentNullException>(() => value.IsNullThrow()); // Should throw
    }

    [Fact]
    public void IsNullThrow_WithNonStringNonNullObject_DoesNotThrow()
    {
        object value = new();
        value.IsNullThrow(); // Should not throw
    }

    [Fact]
    public void IsNullThrow_WithNullList_ThrowsArgumentNullException()
    {
        IList<string> value = null!;
        Assert.Throws<ArgumentNullException>(() => value.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithNonList_DoesNotThrow()
    {
        IList<string> value = new List<string>() { "test" };
        value.IsNullThrow(); // Should not throw
    }

    class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}