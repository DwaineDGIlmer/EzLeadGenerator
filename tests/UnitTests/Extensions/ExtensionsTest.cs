using WebApp.Extensions;

namespace UnitTests.Extensions;

public class ExtensionsTest
{
    [Fact]
    public void IsNull_WithNullObject_ReturnsTrue()
    {
        object obj = null!;
        Assert.True(obj.IsNull());
    }

    [Fact]
    public void IsNull_WithNonNullObject_ReturnsFalse()
    {
        object obj = new();
        Assert.False(obj.IsNull());
    }

    [Fact]
    public void IsNull_WithEmptyString_ReturnsTrue()
    {
        string str = "";
        Assert.True(str.IsNull());
    }

    [Fact]
    public void IsNull_WithNonEmptyString_ReturnsFalse()
    {
        string str = "test";
        Assert.False(str.IsNull());
    }

    [Fact]
    public void IsNullThrow_WithNullObject_ThrowsArgumentNullException()
    {
        object obj = null!;
        Assert.Throws<ArgumentNullException>(() => obj.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithEmptyString_ThrowsArgumentNullException()
    {
        string str = "";
        Assert.Throws<ArgumentNullException>(() => str.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithNonNullObject_ReturnsObject()
    {
        object obj = new();
        Assert.Equal(obj, obj.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_WithNonEmptyString_ReturnsString()
    {
        string str = "test";
        Assert.Equal(str, str.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_List_WithNullList_ThrowsArgumentNullException()
    {
        List<int>? list = null;
        Assert.Throws<ArgumentNullException>(() => list.IsNullThrow());
    }

    [Fact]
    public void IsNullThrow_List_WithNonEmptyList_ReturnsList()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Equal(list, list.IsNullThrow());
    }

    [Fact]
    public void IsNullOrEmpty_WithNullObject_ReturnsTrue()
    {
        object obj = null!;
        Assert.True(obj.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithEmptyString_ReturnsTrue()
    {
        string str = "";
        Assert.True(str.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithNonEmptyString_ReturnsFalse()
    {
        string str = "abc";
        Assert.False(str.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithNonNullObject_ReturnsFalse()
    {
        object obj = new();
        Assert.False(obj.IsNullOrEmpty());
    }

    [Fact]
    public void IsNotNull_WithNullObject_ReturnsFalse()
    {
        object obj = null!;
        Assert.False(obj.IsNotNull());
    }

    [Fact]
    public void IsNotNull_WithNonNullObject_ReturnsTrue()
    {
        object obj = new();
        Assert.True(obj.IsNotNull());
    }

    [Fact]
    public void IsNotNull_WithEmptyString_ReturnsFalse()
    {
        string str = "";
        Assert.False(str.IsNotNull());
    }

    [Fact]
    public void IsNotNull_WithNonEmptyString_ReturnsTrue()
    {
        string str = "abc";
        Assert.True(str.IsNotNull());
    }
}

public class DictionaryExtensionsTest
{
    [Fact]
    public void RemoveNullValues_RemovesEntriesWithNullValues()
    {
        var dict = new Dictionary<string, object?>
        {
            { "a", 1 },
            { "b", null },
            { "c", "test" },
            { "d", null }
        };

        var result = dict.RemoveNullValues();

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("a"));
        Assert.True(result.ContainsKey("c"));
        Assert.False(result.ContainsKey("b"));
        Assert.False(result.ContainsKey("d"));
    }

    [Fact]
    public void RemoveNullValues_WithNoNulls_DoesNothing()
    {
        var dict = new Dictionary<string, object?>
        {
            { "a", 1 },
            { "b", "value" }
        };

        var result = dict.RemoveNullValues();

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("a"));
        Assert.True(result.ContainsKey("b"));
    }

    [Fact]
    public void RemoveNullValues_AllNulls_RemovesAll()
    {
        var dict = new Dictionary<string, object?>
        {
            { "a", null },
            { "b", null }
        };

        var result = dict.RemoveNullValues();

        Assert.Empty(result);
    }
}