using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GLTFTools
{
    public class Accessor
    {
        private int? _byteOffset = null;

        /// <summary>
        /// A typed view into a buffer view. A buffer view contains raw binary data. An accessor provides a typed view into a buffer view or a subset of a buffer view similar to how WebGL's `vertexAttribPointer()` defines an attribute in a buffer
        /// </summary>
        public Accessor() { }

        /// <summary>
        /// The name of the accessor
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The datatype of components in the attribute
        /// </summary>
        [JsonProperty("componentType")]
        public ComponentType ComponentType { get; set; }

        /// <summary>
        /// The number of attributes referenced by this accessor
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// Minimum value of each component in this attribute
        /// </summary>
        [JsonProperty("min")]
        public double[] Min { get; set; }

        /// <summary>
        /// Maximum value of each component in this attribute.
        /// </summary>
        [JsonProperty("max")]
        public double[] Max { get; set; }

        /// <summary>
        /// Specifies if the attribute is a scalar, vector, or matrix
        /// </summary>
        [JsonProperty("type")]
        public GLType Type { get; set; }

        /// <summary>
        /// The index of the buffer view
        /// </summary>
        [JsonProperty("bufferView")]
        public int? BufferView { get; set; }
        
        /// <summary>
        /// The offset relative to the start of the buffer view in bytes
        /// </summary>
        [JsonProperty("byteOffset")]
        public int? ByteOffset
        {
            get => _byteOffset;
            set
            {
                if (!value.HasValue)
                    _byteOffset = value;
                else
                    _byteOffset = value.Value > 0 ? value : 0;
            } 
        }
    }

    internal class AccessorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Accessor accessor = new Accessor();
            
            if (obj["name"] != null)
                accessor.Name = obj["name"].Value<string>();

            if (obj["bufferView"] != null)
                accessor.BufferView = obj["bufferView"].Value<int>();

            if (obj["byteOffset"] != null)
                accessor.BufferView = obj["byteOffset"].Value<int>();

            // Should all be in json
            accessor.ComponentType = ComponentTypeConverter.Parse(obj["componentType"].Value<int>());
            accessor.Type = GLTypeConverter.Parse(obj["type"].Value<string>());
            accessor.Count = obj["count"].Value<int>();
            
            IGLPrimitive GetPrimitive(string propName, ComponentType primitiveType, GLType arrayType)
            {
                var kids = obj[propName].Children();

                if (arrayType == GLType.Scalar)
                {
                    switch(primitiveType)
                    {
                        default:
                            return new Scalar<float>(kids[0].Value<float>());
                        case ComponentType.Byte:
                            return new Scalar<sbyte>(kids[0].Value<sbyte>());
                        case ComponentType.UnsignedByte:
                            return new Scalar<byte>(kids[0].Value<byte>());
                        case ComponentType.Short:
                            return new Scalar<short>(kids[0].Value<short>());
                        case ComponentType.UnsignedShort:
                            return new Scalar<ushort>(kids[0].Value<ushort>());
                        case ComponentType.UnsignedInt:
                            return new Scalar<uint>(kids[0].Value<uint>());
                        case ComponentType.Float:
                            return new Scalar<float>(kids[0].Value<float>());
                    }
                }

                return null;
            }

            //var castType = ComponentTypeConverter.GetType(accessor.ComponentType);
            //var minArray = obj["min"].Children().Select(x => Convert.ChangeType(x, castType)).ToArray();
            //accessor.Min = (Vector3<float>)minArray;

            //var arr = new int[] { 0, 1, 2 };
            //Vector3<int> vec = arr;

            //var min = obj["min"].Children().ToArray();
            //var max = obj["max"];
            
            return accessor;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
