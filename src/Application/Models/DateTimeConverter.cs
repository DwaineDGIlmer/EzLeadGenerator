using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Models;

/// <summary>
/// Converts <see cref="DateTime"/> objects to and from JSON using ISO 8601 format.
/// </summary>
/// <remarks>This converter reads and writes <see cref="DateTime"/> values in the ISO 8601 format,
/// ensuring compatibility with standard JSON date-time representations. It throws a <see cref="JsonException"/> if
/// the JSON value cannot be parsed into a valid <see cref="DateTime"/>.</remarks>
public sealed class DateTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Reads and converts the JSON string representation of a date and time to a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> from which to read the JSON string.</param>
    /// <param name="typeToConvert">The type of the object to convert, which is expected to be <see cref="DateTime"/>.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer.</param>
    /// <returns>A <see cref="DateTime"/> object that represents the date and time parsed from the JSON string.</returns>
    /// <exception cref="JsonException">Thrown if the JSON string is null or cannot be parsed as a valid <see cref="DateTime"/>.</exception>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? throw new JsonException());
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value as a JSON string in ISO 8601 format.
    /// </summary>
    /// <remarks>The <see cref="DateTime"/> value is formatted using the "o" format specifier, which
    /// represents a round-trip date/time pattern.</remarks>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the <see cref="DateTime"/> value is written.</param>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    /// <param name="options">The serialization options to use. This parameter is not used in this method.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o")); // ISO 8601 format
    }
}
