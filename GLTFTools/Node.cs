using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Node
    {
        // TODO: Don't serialize default values

        /// <summary>
        /// A node in the node hierarchy
        /// </summary>
        public Node() { }

        /// <summary>
        /// Name of node
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The index of the camera referenced by this node
        /// </summary>
        [JsonProperty("camera")]
        public int Camera { get; set; }

        /// <summary>
        /// The indices of this node's children
        /// </summary>
        [JsonProperty("children")]
        public int[] Children { get; set; }

        /// <summary>
        /// The index of the skin referenced by this node
        /// </summary>
        [JsonProperty("skin")]
        public int Skin { get; set; }

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order
        /// </summary>
        [JsonProperty("matrix")]
        public Matrix4<float> Matrix { get; set; } = Matrix4<float>.Identity();

        /// <summary>
        /// The index of the mesh in this node
        /// </summary>
        [JsonProperty("mesh")]
        public int Mesh { get; set; }

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w), where w is the scalar
        /// </summary>
        [JsonProperty("rotation")]
        public Vector4<float> Roatation { get; set; } = new Vector4<float>(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The node's non-uniform scale, given as the scaling factors along the x, y, and z axes
        /// </summary>
        [JsonProperty("scale")]
        public Vector3<float> Scale { get; set; } = new Vector3<float>(1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The node's translation along the x, y, and z axes
        /// </summary>
        [JsonProperty("translation")]
        public Vector3<float> Translation { get; set; }

        /// <summary>
        /// The weights of the instantiated Morph Target. Number of elements must match number of Morph Targets of used mesh
        /// </summary>
        [JsonProperty("weights")]
        public float[] Weights { get; set; }
    }
}
