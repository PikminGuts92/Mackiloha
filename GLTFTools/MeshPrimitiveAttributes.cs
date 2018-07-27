using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class MeshPrimitiveAttributes
    {
        // TODO: Implement saving of application specific attributes (e.g. begin with '_')

        /// <summary>
        /// Each value corresponds to mesh attribute semantic and each value is the index of the accessor containing attribute's data
        /// </summary>
        public MeshPrimitiveAttributes() { }

        /// <summary>
        /// XYZ vertex positions
        /// </summary>
        [JsonProperty("POSITION")]
        public int? Position { get; set; }

        /// <summary>
        /// Normalized XYZ vertex normals
        /// </summary>
        [JsonProperty("NORMAL")]
        public int? Normal { get; set; }

        /// <summary>
        /// XYZW vertex tangents where the w component is a sign value (-1 or +1) indicating handedness of the tangent basis
        /// </summary>
        [JsonProperty("TANGENT")]
        public int? Tangent { get; set; }

        /// <summary>
        /// UV texture coordinates for the first set
        /// </summary>
        [JsonProperty("TEXCOORD_0")]
        public int? TextureCoordinate0 { get; set; }

        /// <summary>
        /// UV texture coordinates for the second set
        /// </summary>
        [JsonProperty("TEXCOORD_1")]
        public int? TextureCoordinate1 { get; set; }

        /// <summary>
        /// RGB or RGBA vertex color
        /// </summary>
        [JsonProperty("COLOR_0")]
        public int? Color0 { get; set; }
        
        [JsonProperty("JOINTS_0")]
        public int? Joints0 { get; set; }

        [JsonProperty("WEIGHTS_0")]
        public int? Weights0 { get; set; }
    }
}
