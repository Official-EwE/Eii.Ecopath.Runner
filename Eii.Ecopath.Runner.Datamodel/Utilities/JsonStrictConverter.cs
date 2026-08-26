using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    /// <summary>
    /// Converter that ensures that all declared JSON types are successfully deserialized.
    /// </summary>
    /// <remarks>
    /// Thanks to chatgpt for the inspiration.
    /// </remarks>
    public class JsonStrictConverter<T> : JsonConverter<T> where T : class, new()
    {
        /// <summary>
        /// Reads and converts JSON into an object of type T, validating that no unknown properties are present.
        /// </summary>
        /// <param name="reader">The reader to read JSON from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">Serializer options.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="JsonException">Thrown if unknown properties are found in the JSON.</exception>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var knownProps = typeof(T).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var unknownProps = new List<string>();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (!knownProps.Contains(prop.Name))
                {
                    unknownProps.Add(prop.Name);
                }
            }

            if (unknownProps.Any())
            {
                throw new JsonException($"Unknown properties found in JSON: {string.Join(", ", unknownProps)}");
            }

            var json = doc.RootElement.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options)!;
        }

        /// <summary>
        /// Writes an object of type T as JSON.
        /// </summary>
        /// <param name="writer">The writer to write JSON to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Serializer options.</param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
