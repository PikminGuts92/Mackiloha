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
    }
}
