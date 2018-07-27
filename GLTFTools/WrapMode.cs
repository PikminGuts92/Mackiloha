using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(WrapModeConverter))]
    public enum WrapMode : int
    {
        ClampToEdge = 33071,
        MirroredRepeat = 33648,
        Repeat = 10497
    }

    internal class WrapModeConverter : JsonConverter
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
            if (!Enum.IsDefined(typeof(WrapMode), value))
                throw new JsonReaderException($"\'{reader.Path}\': Value of \'{value}\' is not supported!");

            return (WrapMode)value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(WrapMode))
                throw new JsonWriterException($"\'{writer.Path}\': Value must be a WrapMode!");

            if (!Enum.IsDefined(typeof(WrapMode), value))
                throw new JsonWriterException($"\'{writer.Path}\': Value of \'{value}\' is not supported!");

            writer.WriteValue((int)value);
        }
    }
}
