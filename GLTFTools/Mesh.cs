using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Mesh
    {
        /// <summary>
        /// A set of primitives to be rendered. A node can contain one mesh. A node's transform places the mesh in the scene
        /// </summary>
        public Mesh() { }

        /// <summary>
        /// Name of mesh
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// An array of primitives, each defining geometry to be rendered with a material
        /// </summary>
        [JsonProperty("primitives")]
        public MeshPrimitive[] Primitives { get; set; }

        /// <summary>
        /// Array of weights to be applied to the Morph Targets
        /// </summary>
        [JsonProperty("weights")]
        public float Weights { get; set; }
    }
}
