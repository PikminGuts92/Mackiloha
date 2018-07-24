using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Vector3Converter))]
    public struct Vector3<T> : IGLPrimitive where T : IComparable<T>
    {
        public T X;
        public T Y;
        public T Z;
        
        public Vector3(T x) : this(x, x, x) { }
        
        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    internal class Vector3Converter : JsonConverter
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
