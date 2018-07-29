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

        public T[] ToArray() =>
            new T[]
            {
                this.M11, this.M12, this.M13,
                this.M21, this.M22, this.M23,
                this.M31, this.M32, this.M33
            };

        public T[][] ToArray2D() =>
            new T[][]
            {
                new T[] { this.M11, this.M12, this.M13 },
                new T[] { this.M21, this.M22, this.M23 },
                new T[] { this.M31, this.M32, this.M33 }
            };

        public static Matrix3<float> Identity() =>
            new Matrix3<float>() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f };

        public static implicit operator Matrix3<T>(T[] arr)
        {
            return new Matrix3<T>()
            {
                M11 = arr[0],
                M12 = arr[1],
                M13 = arr[2],
                M21 = arr[3],
                M22 = arr[4],
                M23 = arr[5],
                M31 = arr[6],
                M32 = arr[7],
                M33 = arr[8]
            };
        }

        public static implicit operator Matrix3<T> (T[][] arr)
        {
            return new Matrix3<T>()
            {
                M11 = arr[0][0],
                M12 = arr[0][1],
                M13 = arr[0][2],
                M21 = arr[1][0],
                M22 = arr[1][1],
                M23 = arr[1][2],
                M31 = arr[2][0],
                M32 = arr[2][1],
                M33 = arr[2][2]
            };
        }

        public static implicit operator T[] (Matrix3<T> mat) => mat.ToArray();

        public static implicit operator T[][] (Matrix3<T> mat) => mat.ToArray2D();
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
