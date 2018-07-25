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
                    return RenderMode.TriangleStrip;
            }

            throw new JsonReaderException($"\'{reader.Path}\': Value of \'{reader.Value}\' is not supported!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
