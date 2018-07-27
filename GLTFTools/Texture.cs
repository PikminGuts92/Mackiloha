using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Texture
    {
        /// <summary>
        /// Name of the texture
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The index of the sampler used by this texture. When undefined, a sampler with repeat wrapping and auto filtering should be used
        /// </summary>
        [JsonProperty("sampler")]
        public int? Sampler { get; set; }

        /// <summary>
        /// The index of the image used by this texture
        /// </summary>
        [JsonProperty("source")]
        public int? Source { get; set; }
    }
}
