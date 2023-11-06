using System.Text.Json.Serialization;

namespace P9SongTool.Models
{
    public class LyricEvent
    {
        [JsonPropertyName("Pos")]
        public float[] Position { get; set; } // 3 floats
        [JsonPropertyName("Rot")]
        public float[] Rotation { get; set; } // 4 floats
        public float[] Scale { get; set; } // 3 floats
    }
}
