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

        public T[] ToArray() => new T[] { X, Y, Z };

        public static implicit operator T[] (Vector3<T> vec) => vec.ToArray();

        public static implicit operator Vector3<T> (T[] arr) => new Vector3<T>(arr[0], arr[1], arr[2]);
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
            var vec = (value as dynamic); // TODO: Don't use dynamic?

            writer.WriteStartArray();
            writer.WriteValue(vec.X);
            writer.WriteValue(vec.Y);
            writer.WriteValue(vec.Z);
            writer.WriteEndArray();
        }
    }
}
