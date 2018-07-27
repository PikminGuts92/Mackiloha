using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Vector4Converter))]
    public struct Vector4<T> : IGLPrimitive where T : IComparable<T>
    {
        public T X;
        public T Y;
        public T Z;
        public T W;
        
        public Vector4(T x) : this(x, x, x, x) { }
        
        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    internal class Vector4Converter : JsonConverter
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
            writer.WriteValue(vec.W);
            writer.WriteEndArray();
        }
    }
}
