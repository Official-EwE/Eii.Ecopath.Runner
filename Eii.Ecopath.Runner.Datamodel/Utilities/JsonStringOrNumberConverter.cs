using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    public class JsonStringOrNumberConverter : JsonConverter<string>
    {
        public override string? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetDecimal().ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

                _ => throw new JsonException(
                    $"Expected string or number, got {reader.TokenType}")
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            string value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
