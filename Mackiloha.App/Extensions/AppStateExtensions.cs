using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mackiloha.IO;
using Mackiloha.Milo2;

namespace Mackiloha.App.Extensions
{
    public static class AppStateExtensions
    {
        public static void ExtractMiloContents(this AppState state, string miloPath, string outputDir, bool convertTextures)
        {
            MiloFile miloFile;
            using (var fileStream = state.GetWorkingDirectory().GetStreamForFile(miloPath))
            {
                miloFile = MiloFile.ReadFromStream(fileStream);
            }

            state.UpdateSystemInfo(new SystemInfo()
            {
                Version = miloFile.Version,
                BigEndian = miloFile.BigEndian,
                Platform = GuessPlatform(miloPath, miloFile.Version, miloFile.BigEndian)
            });

            var serializer = state.GetSerializer();

            MiloObjectDir milo;
            using (var miloStream = new MemoryStream(miloFile.Data))
            {
                milo = serializer.ReadFromStream<MiloObjectDir>(miloStream);
            }

            milo.ExtractToDirectory(outputDir, convertTextures, state, state.GetWorkingDirectory());
        }

        private static Platform GuessPlatform(string fileName, int version, bool endian)
        {
            var ext = fileName?.Split('_')?.LastOrDefault()?.ToLower();

            return (ext, version) switch
            {
                ("gc", _) => Platform.GC,
                ("ps2", _) => Platform.PS2,
                ("ps3", _) => Platform.PS3,
                ("ps4", _) => Platform.PS3,
                ("wii", _) => Platform.Wii,
                var (p, v) when p == "xbox" && v <= 24 => Platform.XBOX,
                var (p, v) when p == "xbox" && v >= 25 => Platform.X360,
                // TODO: Determine when XB1
                _ => Platform.PS2
            };
        }

        public static MiloSerializer GetSerializer(this AppState state) => new MiloSerializer(state.SystemInfo);
    }
}
