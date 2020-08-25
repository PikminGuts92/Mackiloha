using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class MeshPrimitive
    {
        /// <summary>
        /// Geometry to be rendered with the given material
        /// </summary>
        public MeshPrimitive() { }

        /// <summary>
        /// Attributes of mesh primitive
        /// </summary>
        [JsonProperty("attributes")]
        public MeshPrimitiveAttributes Attributes { get; set; } // TODO: Force serialization when null

        /// <summary>
        /// The index of the accessor that contains the indices
        /// </summary>
        [JsonProperty("indices")]
        public int? Indices { get; set; }

        /// <summary>
        /// The index of the material to apply to this primitive when rendering
        /// </summary>
        [JsonProperty("material")]
        public int? Material { get; set; }

        /// <summary>
        /// The type of primitives to render
        /// </summary>
        [JsonProperty("mode")]
        public RenderMode Mode { get; set; } = RenderMode.Triangles; // TODO: Don't serialize when = Triangles

        // TODO: Add Targets property
    }
}