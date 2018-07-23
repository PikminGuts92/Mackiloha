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
            // SCALAR, VEC2, VEC3, VEC4, MAT2, MAT3, MAT4
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
