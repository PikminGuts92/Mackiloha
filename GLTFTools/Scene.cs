using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Scene
    {
        /// <summary>
        /// The indices of each root node
        /// </summary>
        [JsonProperty("nodes")]
        public int[] Nodes { get; set; }
    }
}
