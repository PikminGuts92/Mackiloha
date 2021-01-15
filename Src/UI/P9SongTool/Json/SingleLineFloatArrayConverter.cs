using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
// using System.Text.Json; // TODO: Migrate when dotnet is updated https://github.com/dotnet/runtime/issues/1784
//using System.Text.Json.Serialization;

namespace P9SongTool.Json
{
    public class SingleLineFloatArrayConverter : JsonConverter<float[]>
    {
        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Shouldn't be used!
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            if (value is null || value.Length <= 0)
            {
                writer.WriteRawValue("[]");
                return;
            }

            writer.WriteRawValue($"[ {string.Join(", ", value)} ]");
        }
    }
}
