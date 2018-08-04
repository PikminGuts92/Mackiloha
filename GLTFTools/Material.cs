using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Material
    {
        /// <summary>
        /// Name of material
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        
        /// <summary>
        /// A set of parameter values that are used to define the metallic-roughness material model from Physically-Based Rendering (PBR) methodology
        /// </summary>
        [JsonProperty("pbrMetallicRoughness")]
        public PbrMetallicRoughness PbrMetallicRoughness { get; set; }
        
        /// <summary>
        /// The emissive color of the material
        /// </summary>
        [JsonProperty("emissiveFactor")]
        public Vector3<double> EmissiveFactor { get; set; }

        /// <summary>
        /// The alpha rendering mode of the material
        /// </summary>
        [JsonProperty("alphaMode")]
        public AlphaMode AlphaMode { get; set; } = AlphaMode.Opaque;

        /// <summary>
        /// The alpha cutoff value of the material
        /// </summary>
        [JsonProperty("alphaCutoff")]
        public double? AlphaCutOff { get; set; } //= 0.5f;

        /// <summary>
        /// Specifies whether the material is double sided
        /// </summary>
        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; }
    }
}
