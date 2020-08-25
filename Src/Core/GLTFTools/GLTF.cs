using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class GLTF
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };

        public static GLTF FromFile(string path)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                return JsonConvert.DeserializeObject<GLTF>(sr.ReadToEnd(), _jsonSettings);
            }
        }

        public string ToJson() => JsonConvert.SerializeObject(this, _jsonSettings);

        /// <summary>
        /// An array of accessors
        /// </summary>
        [JsonProperty("accessors")]
        public Accessor[] Accessors { get; set; }

        /// <summary>
        /// Metadata about the glTF asset
        /// </summary>
        [JsonProperty("asset")]
        public Asset Asset { get; set; }

        /// <summary>
        /// An array of buffers
        /// </summary>
        [JsonProperty("buffers")]
        public Buffer[] Buffers { get; set; }

        /// <summary>
        /// An array of buffer views
        /// </summary>
        [JsonProperty("bufferViews")]
        public BufferView[] BufferViews { get; set; }

        /// <summary>
        /// An array of images
        /// </summary>
        [JsonProperty("images")]
        public Image[] Images { get; set; }

        /// <summary>
        /// An array of materials
        /// </summary>
        [JsonProperty("materials")]
        public Material[] Materials { get; set; }

        /// <summary>
        /// An array of meshes
        /// </summary>
        [JsonProperty("meshes")]
        public Mesh[] Meshes { get; set; }

        /// <summary>
        /// An array of nodes
        /// </summary>
        [JsonProperty("nodes")]
        public Node[] Nodes { get; set; }

        /// <summary>
        /// An array of samplers
        /// </summary>
        [JsonProperty("samplers")]
        public Sampler[] Samplers { get; set; }

        /// <summary>
        /// The index of the default scene
        /// </summary>
        [JsonProperty("scene")]
        public int Scene { get; set; }

        /// <summary>
        /// An array of scenes
        /// </summary>
        [JsonProperty("scenes")]
        public Scene[] Scenes { get; set; }

        /// <summary>
        /// An array of skins.  A skin is defined by joints and matrices.
        /// </summary>
        [JsonProperty("skins")]
        public Skin[] Skins { get; set; }

        /// <summary>
        /// An array of textures
        /// </summary>
        [JsonProperty("textures")]
        public Texture[] Textures { get; set; }
    }
}
