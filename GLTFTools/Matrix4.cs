using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Matrix4Converter))]
    public struct Matrix4<T> : IGLPrimitive where T : IComparable<T>
    {
        public T M11, M12, M13, M14;
        public T M21, M22, M23, M24;
        public T M31, M32, M33, M34;
        public T M41, M42, M43, M44;

        public static Matrix4<float> Identity() =>
            new Matrix4<float>() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };
    }

    internal class Matrix4Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
