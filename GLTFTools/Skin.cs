using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GLTFTools
{
    public class Skin
    {
        /// <summary>
        /// Joints and matrices defining a skin.
        /// </summary>
        public Skin() { }

        /// <summary>
        /// The name of the skin
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The index of the accessor containing the floating-point 4x4 inverse-bind matrices.  The default is that each matrix is a 4x4 identity matrix, which implies that inverse-bind matrices were pre-applied.
        /// </summary>
        [JsonProperty("inverseBindMatrices")]
        public int? InverseBindMatrics { get; set; }

        /// <summary>
        /// The index of the node used as a skeleton root. The node must be the closest common root of the joints hierarchy or a direct or indirect parent node of the closest common root.
        /// </summary>
        [JsonProperty("skeleton")]
        public int? Skeleton { get; set; }

        /// <summary>
        /// Indices of skeleton nodes, used as joints in this skin.  The array length must be the same as the `count` property of the `inverseBindMatrices` accessor (when defined).
        /// </summary>
        [JsonProperty("joints")]
        public int[] Joints { get; set; }
    }
}
