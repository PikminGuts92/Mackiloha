using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class BufferView
    {
        private const int STRIDE_MULTIPLE = 4;

        private int _byteOffset = 0;
        private int _byteLength = 1;
        private int _byteStride = 4;

        /// <summary>
        /// That name of the buffer view
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The index of the buffer
        /// </summary>
        [JsonProperty("buffer")]
        public int Buffer { get; set; }

        /// <summary>
        /// The offset into the buffer in bytes
        /// </summary>
        [JsonProperty("byteOffset")]
        public int ByteOffset
        {
            get => _byteOffset;
            set => _byteOffset = (value > 0) ? value : 0;
        }

        /// <summary>
        /// The length of the bufferView in bytes
        /// </summary>
        [JsonProperty("byteLength")]
        public int ByteLength
        {
            get => _byteLength;
            set => _byteLength = (value > 0) ? value : 1;
        }

        /// <summary>
        /// The stride, in bytes, between vertex buffers. Multiple of 4 (Min = 4, Max = 252)
        /// </summary>
        [JsonProperty("byteStride")]
        public int ByteStride
        {
            get => _byteStride;
            set
            {
                if (value < 4) _byteStride = 4;
                else if (value > 252) _byteStride = 252;
                else _byteStride = ((value % STRIDE_MULTIPLE) > 0)
                        ? (value + STRIDE_MULTIPLE - (value % STRIDE_MULTIPLE))
                        : value;
            }
        }

        /// <summary>
        /// The target that the GPU buffer should be bound to
        /// </summary>
        [JsonProperty("target")]
        public TargetBuffer Target { get; set; }
    }
}
