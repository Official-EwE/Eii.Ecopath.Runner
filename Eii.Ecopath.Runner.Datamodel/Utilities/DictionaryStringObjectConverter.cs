using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    /// <summary>
    /// Converter for string:object dictionaries that recursively parses
    /// any JsonElement object for the underlying .NET types.
    /// </summary>
    /// <remarks>
    /// Thanks to chatgpt for the inspiration.
    /// </remarks>
    public class DictionaryStringObjectConverter : JsonConverter<Dictionary<string, object>>
    {
        /// <summary>
        /// Reads and converts JSON into a Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="reader">The reader to read JSON from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">Serializer options.</param>
        /// <returns>A dictionary containing the deserialized JSON data.</returns>
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            return ConvertJsonElementToDictionary(doc.RootElement);
        }

        /// <summary>
        /// Writes a Dictionary&lt;string, object&gt; as JSON.
        /// </summary>
        /// <param name="writer">The writer to write JSON to.</param>
        /// <param name="value">The dictionary to serialize.</param>
        /// <param name="options">Serializer options.</param>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

        /// <summary>
        /// Converts a JsonElement representing a JSON object into a Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="element">The JsonElement to convert.</param>
        /// <returns>A dictionary containing the converted data.</returns>
        /// <exception cref="JsonException">Thrown if the element is not a JSON object.</exception>
        private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                throw new JsonException("Expected JSON object");

            var dict = new Dictionary<string, object>();
            foreach (JsonProperty property in element.EnumerateObject())
            {
                var target = ConvertJsonElement(property.Value);
                if (target != null)
                    dict[property.Name] = target;
            }
            return dict;
        }

        /// <summary>
        /// Converts a JsonElement into its corresponding .NET object type.
        /// </summary>
        /// <param name="element">The JsonElement to convert.</param>
        /// <returns>The converted .NET object, or null if the element is null or undefined.</returns>
        private static object? ConvertJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
                return null;

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToArray(),
                JsonValueKind.Object => ConvertJsonElementToDictionary(element), // Recursive call for nested objects
                _ => element.ToString()
            };
        }
    }
}
