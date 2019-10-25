using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Mackiloha.IO;
using SuperFreqCLI.Models;

namespace SuperFreqCLI.Options
{
    internal class GameOptions
    {
        [Option("miloVersion", Default = 10, HelpText = "Milo archive version (10, 24, 25)")]
        public int MiloVersion { get; set; }

        [Option("bigEndian", Default = false, HelpText = "Use big endian serialization")]
        public bool BigEndian { get; set; }

        [Option("platform", Default = Platform.PS2, HelpText = "Platform (ps2, x360)")]
        public Platform Platform { get; set; }

        [Option("preset", HelpText = "Game preset (gh1, gh2, gh80s, gh2_x360)")]
        public string Preset { get; set; }

        protected void UpdateOptionsIfPreset()
        {
            var presetValue = Preset?.Trim().ToLower();

            if (presetValue == null)
            {
                // Do nothing
                return;
            }

            var config = MiloConfig.Presets
                    .FirstOrDefault(x => x.Games.Any(y => y == presetValue));

            if (config == null)
            {
                // Preset no found
                // TODO: Throw exception?
                return;
            }

            // Updates options
            MiloVersion = config.MiloVersion;
            BigEndian = config.BigEndian;
            Platform = config.Platform;
        }

        protected SystemInfo GetSystemInfo() => new SystemInfo()
        {
            Version = MiloVersion,
            BigEndian = BigEndian,
            Platform = Platform
        };
    }
}
