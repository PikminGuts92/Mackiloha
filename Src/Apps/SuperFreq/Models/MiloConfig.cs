﻿using Mackiloha.IO;

namespace SuperFreq.Models;

internal class MiloConfig
{
    public string Name { get; set; }
    public string[] Games { get; set; }
    public int MiloVersion { get; set; }
    public bool BigEndian { get; set; }
    public Platform Platform { get; set; }

    public static List<MiloConfig> Presets{ get; } = new List<MiloConfig>()
    {
        new MiloConfig()
        {
            Name = "GH1",
            Games = new[] { "gh1" },
            MiloVersion = 10,
            BigEndian = false,
            Platform = Platform.PS2
        },
        new MiloConfig()
        {
            Name = "GH2/GH80s",
            Games = new[] { "gh2", "gh80s" },
            MiloVersion = 24,
            BigEndian = false,
            Platform = Platform.PS2
        },
        new MiloConfig()
        {
            Name = "GH2 X360",
            Games = new[] { "gh2_x360" },
            MiloVersion = 25,
            BigEndian = false,
            Platform = Platform.X360
        }
    };

    public static MiloConfig Default { get; } = Presets[1];
}
