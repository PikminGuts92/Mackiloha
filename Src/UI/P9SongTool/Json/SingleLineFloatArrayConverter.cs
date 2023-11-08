using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P9SongTool.Json;

public class SingleLineFloatArrayConverter : JsonConverter<float[]>
{
    protected readonly CultureInfo CurrentCulture = new CultureInfo("en-US");

    public override float[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Just use default (separate context to avoid circular reference)
        return JsonSerializer.Deserialize<float[]>(ref reader, PrimitiveJsonContext.Default.SingleArray);
    }

    public override void Write(Utf8JsonWriter writer, float[] value, JsonSerializerOptions options)
    {
        if (value is null || value.Length <= 0)
        {
            writer.WriteRawValue("[]");
            return;
        }

        var valuesWithPeriod = value
            .Select(x => x.ToString(CurrentCulture));

        writer.WriteRawValue($"[ {string.Join(", ", valuesWithPeriod)} ]");
    }
}
