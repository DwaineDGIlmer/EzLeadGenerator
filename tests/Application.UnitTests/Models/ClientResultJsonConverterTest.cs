using Application.Models;
using System.Text.Json;

namespace Application.UnitTests.Models;

// Dummy ClientResult<T> for testing purposes
public class ClientResult<T>(T value)
{
    public T Value { get; } = value;
}

public class ClientResultJsonConverterTest
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ClientResultJsonConverter<string>() }
    };

    [Fact]
    public void Read_ShouldDeserializeValueProperty()
    {
        var json = "{\"value\":\"test-data\"}";
        var result = JsonSerializer.Deserialize<ClientResult<string>>(json, _options);

        Assert.NotNull(result);
        Assert.Equal("test-data", result.Value);
    }

    [Fact]
    public void Read_ShouldReturnNullIfNoValueProperty()
    {
        var json = "{\"other\":\"data\"}";
        var result = JsonSerializer.Deserialize<ClientResult<string>>(json, _options);

        Assert.NotNull(result);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ReadAndWrite_ShouldRoundTrip()
    {
        var original = new ClientResult<string>("roundtrip");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<ClientResult<string>>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Value, deserialized.Value);
    }
}