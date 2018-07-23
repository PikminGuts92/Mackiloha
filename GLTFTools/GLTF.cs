using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class GLTF
    {
        /// <summary>
        /// An array of accessors
        /// </summary>
        [JsonProperty("accessors")]
        public Accessor[] Accessors { get; set; }

        /// <summary>
        /// Metadata about the glTF asset
        /// </summary>
        [JsonProperty("asset")]
        public Asset Asset { get; set; }

        /// <summary>
        /// An array of buffers
        /// </summary>
        [JsonProperty("buffers")]
        public Buffer[] Buffers { get; set; }

        /// <summary>
        /// An array of buffer views
        /// </summary>
        [JsonProperty("bufferViews")]
        public BufferView[] BufferViews { get; set; }

        /// <summary>
        /// An array of images
        /// </summary>
        [JsonProperty("images")]
        public Image[] Images { get; set; }
    }
}
