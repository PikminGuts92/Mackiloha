using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace P9SongTool.Models
{
    public class P9Song
    {
        public string Name { get; set; }
        public SongPreferences Preferences { get; set; }
        [JsonPropertyName("LyricConfigs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LyricConfig[] LyricConfigurations { get; set; }
    }
}
