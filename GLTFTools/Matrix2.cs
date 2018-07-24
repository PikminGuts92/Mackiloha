using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Matrix2Converter))]
    public struct Matrix2<T> : IGLPrimitive where T : IComparable<T>
    {
        public T M11, M12;
        public T M21, M22;

        public static Matrix2<float> Identity() =>
            new Matrix2<float>() { M11 = 1.0f, M22 = 1.0f };
    }

    internal class Matrix2Converter : JsonConverter
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
