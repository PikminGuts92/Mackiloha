using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public enum RenderMode : int
    {
        Points,
        Lines,
        LineLoop,
        LineStrip,
        Triangles,
        TriangleStrip,
        TriangleFan
    }

    internal class RenderModeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new JsonReaderException($"\'{reader.Path}\': Value must be a string!");

            switch (((string)reader.Value).ToUpper())
            {
                case "POINTS":
                    return RenderMode.Points;
                case "LINES":
                    return RenderMode.Lines;
                case "LINE_LOOP":
                    return RenderMode.LineLoop;
                case "LINE_STRIP":
                    return RenderMode.LineStrip;
                case "TRIANGLES":
                    return RenderMode.Triangles;
                case "TRIANGLE_STRIP":
                    return RenderMode.TriangleStrip;
                case "TRIANGLE_FAN":
                    return RenderMode.TriangleFan;
            }

            throw new JsonReaderException($"\'{reader.Path}\': Value of \'{reader.Value}\' is not supported!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(RenderMode))
                throw new JsonWriterException($"\'{writer.Path}\': Value must be a RenderMode!");

            string strValue;
            switch ((RenderMode)value)
            {
                case RenderMode.Points:
                    strValue = "POINTS";
                    break;
                case RenderMode.Lines:
                    strValue = "LINES";
                    break;
                case RenderMode.LineLoop:
                    strValue = "LINE_LOOP";
                    break;
                case RenderMode.LineStrip:
                    strValue = "LINE_STRIP";
                    break;
                case RenderMode.Triangles:
                    strValue = "TRIANGLES";
                    break;
                case RenderMode.TriangleStrip:
                    strValue = "TRIANGLE_STRIP";
                    break;
                case RenderMode.TriangleFan:
                    strValue = "TRIANGLE_FAN";
                    break;
                default:
                    throw new JsonWriterException($"\'{writer.Path}\': Value of \'{value}\' is not supported!");
            }

            writer.WriteValue(strValue);
        }
    }
}
