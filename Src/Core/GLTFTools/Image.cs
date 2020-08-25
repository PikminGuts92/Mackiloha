using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Image
    {
        /// <summary>
        /// The name of the image
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The uri of the image
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; } // TODO: Force write

        /// <summary>
        /// The image's MIME type
        /// </summary>
        [JsonProperty("mimeType")]
        public MimeType? MimeType { get; set; }

        /// <summary>
        /// The index of the bufferView that contains the image. Use this instead of the image's uri property
        /// </summary>
        [JsonProperty("bufferView")]
        public int? BufferView { get; set; } // TODO: Force write
    }
}
