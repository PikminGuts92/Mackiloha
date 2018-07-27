using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(MagFilterConverter))]
    public enum MagFilter : int
    {
        Nearest = 9728,
        Linear
    }

    internal class MagFilterConverter : JsonConverter
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
            if (!Enum.IsDefined(typeof(MagFilter), value))
                throw new JsonReaderException($"\'{reader.Path}\': Value of \'{value}\' is not supported!");

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(MagFilter))
                throw new JsonWriterException($"\'{writer.Path}\': Value must be a MagFilter!");

            if (!Enum.IsDefined(typeof(MagFilter), value))
                throw new JsonWriterException($"\'{writer.Path}\': Value of \'{value}\' is not supported!");

            writer.WriteValue((int)value);
        }
    }
}
