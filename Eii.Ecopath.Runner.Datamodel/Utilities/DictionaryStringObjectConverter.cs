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
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            return ConvertJsonElementToDictionary(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

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
