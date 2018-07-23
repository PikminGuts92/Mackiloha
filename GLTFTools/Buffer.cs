using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Buffer
    {
        private int _byteLength = 1;

        /// <summary>
        /// A buffer points to binary geometry, animation, or skins
        /// </summary>
        public Buffer() { }

        /// <summary>
        /// The name of the buffer
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The length of the buffer in bytes (Must be at least 1)
        /// </summary>
        [JsonProperty("byteLength")]
        public int ByteLength
        {
            get => _byteLength;
            set => _byteLength = (value > 0) ? value : 1;
        }

        /// <summary>
        /// The uri of the buffer
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
