using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EwERunConsole.Utilities
{
    /// <summary>
    /// Converter that ensures that all declared JSON types are successfully deserialized.
    /// </summary>
    /// <remarks>
    /// Thanks to chatgpt for the inspiration.
    /// </remarks>
    public class JsonStrictConverter<T> : JsonConverter<T> where T : class, new()
    {
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

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
