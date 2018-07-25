using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(TargetBufferConverter))]
    public enum TargetBuffer : int
    {
        ArrayBuffer = 34962,
        ElementArrayBuffer
    }

    internal class TargetBufferConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new JsonReaderException($"\'{reader.Path}\': Value must be a number!");

            var value = Convert.ToInt32(reader.Value);
            if (!Enum.IsDefined(typeof(TargetBuffer), value))
                throw new JsonReaderException($"\'{reader.Path}\': Value of \'{value}\' is not supported!");

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
