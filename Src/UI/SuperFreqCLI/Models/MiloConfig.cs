using Mackiloha.IO;

namespace SuperFreqCLI.Models
{
    internal class MiloConfig
    {
        public string[] Games { get; set; }
        public int MiloVersion { get; set; }
        public bool BigEndian { get; set; }
        public Platform Platform { get; set; }

        public static List<MiloConfig> Presets{ get; } = new List<MiloConfig>()
        {
            new MiloConfig()
            {
                Games = new[] { "gh1" },
                MiloVersion = 10,
                BigEndian = false,
                Platform = Platform.PS2
            },
            new MiloConfig()
            {
                Games = new[] { "gh2", "gh80s" },
                MiloVersion = 24,
                BigEndian = false,
                Platform = Platform.PS2
            },
            new MiloConfig()
            {
                Games = new[] { "gh2_x360" },
                MiloVersion = 25,
                BigEndian = false,
                Platform = Platform.X360
            }
        };

        public static MiloConfig Default { get; } = Presets[1];
    }
}
