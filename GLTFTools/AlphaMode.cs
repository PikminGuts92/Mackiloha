using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(AlphaModeConverter))]
    public enum AlphaMode : int
    {
        Opaque,
        Mask,
        Blend
    }

    internal class AlphaModeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new JsonReaderException($"\'{reader.Path}\': Value must be a string!");

            return Parse((string)reader.Value, reader.Path);
        }

        public static AlphaMode Parse(string value, string readerPath = "")
        {
            switch (value.ToUpper())
            {
                case "OPAQUE":
                    return AlphaMode.Opaque;
                case "MASK":
                    return AlphaMode.Mask;
                case "BLEND":
                    return AlphaMode.Blend;
            }

            throw new JsonReaderException($"\'{readerPath}\': Value of \'{value}\' is not supported!");
        }

        public static bool TryParse(string value, out AlphaMode type)
        {
            switch (value.ToUpper())
            {
                case "OPAQUE":
                    type = AlphaMode.Opaque;
                    return true;
                case "MASK":
                    type = AlphaMode.Mask;
                    return true;
                case "BLEND":
                    type = AlphaMode.Blend;
                    return true;
                default:
                    type = AlphaMode.Opaque;
                    return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(AlphaMode))
                throw new JsonWriterException($"\'{writer.Path}\': Value must be an AlphaMode!");

            string strValue;
            switch ((AlphaMode)value)
            {
                case AlphaMode.Opaque:
                    strValue = "OPAQUE";
                    break;
                case AlphaMode.Mask:
                    strValue = "MASK";
                    break;
                case AlphaMode.Blend:
                    strValue = "BLEND";
                    break;
                default:
                    throw new JsonWriterException($"\'{writer.Path}\': Value of \'{value}\' is not supported!");
            }

            writer.WriteValue(strValue);
        }
    }
}
