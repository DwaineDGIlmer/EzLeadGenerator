using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Models;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="ClientResult{T}"/> objects.
/// </summary>
/// <remarks>This converter handles the JSON structure where the main data is encapsulated within a "value"
/// property. It is used to correctly serialize and deserialize <see cref="ClientResult{T}"/> objects to and from
/// JSON.</remarks>
/// <typeparam name="T">The type of the value contained within the <see cref="ClientResult{T}"/>.</typeparam>
public class ClientResultJsonConverter<T> : JsonConverter<ClientResult<T>>
{
    /// <summary>
    /// Reads and converts the JSON data to a <see cref="ClientResult{T}"/> object.
    /// </summary>
    /// <remarks>This method attempts to extract the "value" property from the JSON and convert it to the
    /// specified type. If the "value" property is not found, the method returns <see langword="null"/>.</remarks>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of the object to convert to.</param>
    /// <param name="options">Options to control the behavior during deserialization.</param>
    /// <returns>A <see cref="ClientResult{T}"/> containing the deserialized value if the "value" property is present; otherwise,
    /// <see langword="null"/>.</returns>
    public override ClientResult<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize the JSON into a dictionary or intermediate object
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

        // Extract the "value" property from the JSON
        if (jsonElement.TryGetProperty("value", out var valueElement))
        {
            var value = JsonSerializer.Deserialize<T>(valueElement.GetRawText(), options);

            // Use Activator.CreateInstance to create an instance of ClientResult<T>
            return Activator.CreateInstance(typeof(ClientResult<T>), value) as ClientResult<T>;
        }

        return null;
    }

    /// <summary>
    /// Writes the specified <see cref="ClientResult{T}"/> object to the provided <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the JSON representation of the object is written. Cannot be null.</param>
    /// <param name="value">The <see cref="ClientResult{T}"/> object to serialize. Cannot be null.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing the object. Can be null.</param>
    public override void Write(Utf8JsonWriter writer, ClientResult<T> value, JsonSerializerOptions options)
    {
        // Serialize the ClientResult<T> object back to JSON
        JsonSerializer.Serialize(writer, new { value = value.Value }, options);
    }
}