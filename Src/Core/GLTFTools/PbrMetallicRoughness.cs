using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class PbrMetallicRoughness
    {
        [JsonProperty("baseColorTexture")]
        public BaseColorTexture BaseColorTexture { get; set; }

        [JsonProperty("baseColorFactor")]
        public Vector4<double> BaseColorFactor { get; set; } = new Vector4<double>(1.0f);

        [JsonProperty("metallicFactor")]
        public double MetallicFactor { get; set; } = 0.0f;

        [JsonProperty("roughnessFactor")]
        public double RoughnessFactor { get; set; } = 0.0f;
    }
}
