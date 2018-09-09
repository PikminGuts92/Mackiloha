using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Vector2Converter))]
    public struct Vector2<T> : IGLPrimitive
    {
        public T X;
        public T Y;
        
        public Vector2(T x) : this(x, x) { }

        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public T[] ToArray() => new T[] { X, Y };

        public static implicit operator T[] (Vector2<T> vec) => vec.ToArray();

        public static implicit operator Vector2<T> (T[] arr) => new Vector2<T>(arr[0], arr[1]);
    }

    internal class Vector2Converter : JsonConverter
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
