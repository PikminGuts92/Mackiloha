using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Matrix3Converter))]
    public struct Matrix3<T> : IGLPrimitive where T : IComparable<T>
    {
        public T M11, M12, M13;
        public T M21, M22, M23;
        public T M31, M32, M33;

        public static Matrix3<float> Identity() =>
            new Matrix3<float>() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f };
    }

    internal class Matrix3Converter : JsonConverter
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
