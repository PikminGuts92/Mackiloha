using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class BaseColorTexture
    {
        [JsonProperty("index")]
        public int Index { get; set; }
    }
}
