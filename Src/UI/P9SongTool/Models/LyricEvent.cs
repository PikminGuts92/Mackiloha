using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace P9SongTool.Models
{
    public class LyricEvent
    {
        public float Time { get; set; }
        [JsonPropertyName("Pos")]
        [JsonProperty("Pos")]
        public float[] Position { get; set; } // 3 floats
        [JsonPropertyName("Rot")]
        [JsonProperty("Rot")]
        public float[] Rotation { get; set; } // 4 floats
        public float[] Scale { get; set; } // 3 floats
    }
}
