using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(GLTypeConverter))]
    public enum GLType : int
    {
        Scalar,
        Vector2,
        Vector3,
        Vector4,
        Matrix2,
        Matrix3,
        Matrix4
    }

    internal class GLTypeConverter : JsonConverter
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

        public static GLType Parse(string value, string readerPath = "")
        {
            switch (value.ToUpper())
            {
                case "SCALAR":
                    return GLType.Scalar;
                case "VEC2":
                    return GLType.Vector2;
                case "VEC3":
                    return GLType.Vector3;
                case "VEC4":
                    return GLType.Vector4;
                case "MAT2":
                    return GLType.Matrix2;
                case "MAT3":
                    return GLType.Matrix3;
                case "MAT4":
                    return GLType.Matrix4;
            }

            throw new JsonReaderException($"\'{readerPath}\': Value of \'{value}\' is not supported!");
        }

        public static bool TryParse(string value, out GLType type)
        {
            switch (value.ToUpper())
            {
                case "SCALAR":
                    type = GLType.Scalar;
                    return true;
                case "VEC2":
                    type = GLType.Vector2;
                    return true;
                case "VEC3":
                    type = GLType.Vector3;
                    return true;
                case "VEC4":
                    type = GLType.Vector4;
                    return true;
                case "MAT2":
                    type = GLType.Matrix2;
                    return true;
                case "MAT3":
                    type = GLType.Matrix3;
                    return true;
                case "MAT4":
                    type = GLType.Matrix4;
                    return true;
                default:
                    type = GLType.Scalar;
                    return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
