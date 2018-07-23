using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Accessor
    {
        private int _byteOffset = 0;

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
        public GLPrimitive Min { get; set; }

        /// <summary>
        /// Maximum value of each component in this attribute.
        /// </summary>
        [JsonProperty("max")]
        public GLPrimitive Max { get; set; }

        /// <summary>
        /// Specifies if the attribute is a scalar, vector, or matrix
        /// </summary>
        [JsonProperty("type")]
        public GLType Type { get; set; }

        /// <summary>
        /// The index of the buffer view
        /// </summary>
        [JsonProperty("bufferView")]
        public int BufferView { get; set; }
        
        /// <summary>
        /// The offset relative to the start of the buffer view in bytes
        /// </summary>
        [JsonProperty("byteOffset")]
        public int ByteOffset
        {
            get => _byteOffset;
            set => _byteOffset = (value > 0) ? value : 0;
        }
    }
}
