using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Vector3Converter))]
    public struct Vector3<T> : IGLPrimitive
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
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonReaderException($"\'{reader.Path}\': Value must be an array!");

            Vector3<double> vec = new Vector3<double>();
            vec.X = reader.ReadAsDouble().Value;
            vec.Y = reader.ReadAsDouble().Value;
            vec.Z = reader.ReadAsDouble().Value;
            reader.Read();

            return vec;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic vec = value;

            writer.WriteStartArray();
            writer.WriteValue(vec.X);
            writer.WriteValue(vec.Y);
            writer.WriteValue(vec.Z);
            writer.WriteEndArray();
        }
    }
}
