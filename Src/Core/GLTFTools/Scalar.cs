using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(ScalarConverter))]
    public struct Scalar<T> : IGLPrimitive
    {
        public T Value;
        
        public Scalar(T value)
        {
            Value = value;
        }

        public static implicit operator Scalar<T>(T value) => new Scalar<T>(value);

        public static implicit operator T(Scalar<T> t) => t.Value;
    }

    internal class ScalarConverter : JsonConverter
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
