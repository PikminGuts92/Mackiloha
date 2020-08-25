using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Matrix2Converter))]
    public struct Matrix2<T> : IGLPrimitive
    {
        public T M11, M12;
        public T M21, M22;

        public T[] ToArray() =>
            new T[]
            {
                this.M11, this.M12,
                this.M21, this.M22
            };

        public T[][] ToArray2D() =>
            new T[][]
            {
                new T[] { this.M11, this.M12 },
                new T[] { this.M21, this.M22 }
            };

        public static Matrix2<float> Identity() =>
            new Matrix2<float>() { M11 = 1.0f, M22 = 1.0f };

        public static implicit operator Matrix2<T>(T[] arr)
        {
            return new Matrix2<T>()
            {
                M11 = arr[0],
                M12 = arr[1],
                M21 = arr[2],
                M22 = arr[3]
            };
        }

        public static implicit operator Matrix2<T> (T[][] arr)
        {
            return new Matrix2<T>()
            {
                M11 = arr[0][0],
                M12 = arr[0][1],
                M21 = arr[1][0],
                M22 = arr[1][1]
            };
        }

        public static implicit operator T[] (Matrix2<T> mat) => mat.ToArray();

        public static implicit operator T[][] (Matrix2<T> mat) => mat.ToArray2D();
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
