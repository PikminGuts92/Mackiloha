using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(Matrix4Converter))]
    public struct Matrix4<T> : IGLPrimitive where T : IComparable<T>
    {
        public T M11, M12, M13, M14;
        public T M21, M22, M23, M24;
        public T M31, M32, M33, M34;
        public T M41, M42, M43, M44;

        public T[] ToArray() =>
            new T[]
            {
                this.M11, this.M12, this.M13, this.M14,
                this.M21, this.M22, this.M23, this.M24,
                this.M31, this.M32, this.M33, this.M34,
                this.M41, this.M42, this.M43, this.M44
            };

        public T[][] ToArray2D() =>
            new T[][]
            {
                new T[] { this.M11, this.M12, this.M13, this.M14 },
                new T[] { this.M21, this.M22, this.M23, this.M24 },
                new T[] { this.M31, this.M32, this.M33, this.M34 },
                new T[] { this.M41, this.M42, this.M43, this.M44 },
            };

        public static Matrix4<float> Identity() =>
            new Matrix4<float>() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };
        
        public static implicit operator Matrix4<T>(T[][] arr)
        {
            int[][] mat = new int[][] { new int[] { 0 } };

            // TODO: Add length check
            return new Matrix4<T>()
            {
                M11 = arr[0][0],
                M12 = arr[0][1],
                M13 = arr[0][2],
                M14 = arr[0][3],
                M21 = arr[1][0],
                M22 = arr[1][1],
                M23 = arr[1][2],
                M24 = arr[1][3],
                M31 = arr[2][0],
                M32 = arr[2][1],
                M33 = arr[2][2],
                M34 = arr[2][3],
                M41 = arr[3][0],
                M42 = arr[3][1],
                M43 = arr[3][2],
                M44 = arr[3][3],
            };
        }

        public static implicit operator T[] (Matrix4<T> mat) =>
            new T[]
            {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };

        public static implicit operator T[][](Matrix4<T> mat) =>
            new T[][]
            {
                new T[] { mat.M11, mat.M12, mat.M13, mat.M14 },
                new T[] { mat.M21, mat.M22, mat.M23, mat.M24 },
                new T[] { mat.M31, mat.M32, mat.M33, mat.M34 },
                new T[] { mat.M41, mat.M42, mat.M43, mat.M44 },
            };
    }

    internal class Matrix4Converter : JsonConverter
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
            // TODO: Check for null rows?
            var mat = (value as dynamic); // TODO: Don't use dynamic?

            writer.WriteStartArray();
            writer.WriteValue(mat.M11);
            writer.WriteValue(mat.M12);
            writer.WriteValue(mat.M13);
            writer.WriteValue(mat.M14);
            writer.WriteValue(mat.M21);
            writer.WriteValue(mat.M22);
            writer.WriteValue(mat.M23);
            writer.WriteValue(mat.M24);
            writer.WriteValue(mat.M31);
            writer.WriteValue(mat.M32);
            writer.WriteValue(mat.M33);
            writer.WriteValue(mat.M34);
            writer.WriteValue(mat.M41);
            writer.WriteValue(mat.M42);
            writer.WriteValue(mat.M43);
            writer.WriteValue(mat.M44);
            writer.WriteEndArray();
        }
    }
}
