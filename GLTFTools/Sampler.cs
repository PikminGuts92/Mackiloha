using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Sampler
    {
        /// <summary>
        /// A sampler contains properties for texture filtering and wrapping modes
        /// </summary>
        public Sampler() { }

        /// <summary>
        /// Magnification filter
        /// </summary>
        [JsonProperty("magFilter")]
        public MagFilter MagFilter { get; set; }

        /// <summary>
        /// Minification filter
        /// </summary>
        [JsonProperty("minFilter")]
        public MinFilter MinFilter { get; set; }

        /// <summary>
        /// S wrapping mode
        /// </summary>
        [JsonProperty("wrapS")]
        public WrapMode WrapS { get; set; } = WrapMode.Repeat;

        /// <summary>
        /// T wrapping mode
        /// </summary>
        [JsonProperty("wrapT")]
        public WrapMode WrapT { get; set; } = WrapMode.Repeat;
    }
}
