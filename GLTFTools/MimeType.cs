using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(MimeTypeConverter))]
    public enum MimeType : int
    {
        Image_Png,
        Image_Jpeg
    }

    internal class MimeTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new JsonReaderException($"\'{reader.Path}\' must be a string!");

            switch (((string)reader.Value).ToLower())
            {
                case "image/jpeg":
                    return MimeType.Image_Jpeg;
                case "image/png":
                    return MimeType.Image_Png;
            }

            throw new JsonReaderException($"\'{reader.Path}\': Value of \'{reader.Value}\' is not supported!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(MimeType))
                throw new JsonWriterException($"\'{writer.Path}\': Value must be a MimeType!");

            string strValue;
            switch ((MimeType)value)
            {
                case MimeType.Image_Jpeg:
                    strValue = "image/jpeg";
                    break;
                case MimeType.Image_Png:
                    strValue = "image/png";
                    break;
                default:
                    throw new JsonWriterException($"\'{writer.Path}\': Value of \'{value}\' is not supported!");
            }

            writer.WriteValue(strValue);
        }
    }
}
