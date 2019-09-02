using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mackiloha.App.Metadata;
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

        public static void BuildMiloArchive(this AppState state, string dirPath, string outputPath)
        {
            var directoryTypes = new[]
            {
                // GH2 PS2
                "BandCharacter",
                "BandCrowdMeterDir",
                "BandLeadMeter",
                "BandScoreDisplay",
                "BandStarMeterDir",
                "BandStreakDisplay",
                "CharClipSet",
                "Character",
                "ObjectDir",
                "PanelDir",
                "RndDir",
                "WorldDir",
                //"WorldFx" // TODO: Find better way to find directory entry
            };

            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException();

            // Only finds files at 2nd depth
            var dirDepth = dirPath
                .Split(Path.DirectorySeparatorChar)
                .Count();

            var allFiles = Directory
                .GetFiles(dirPath, "*", SearchOption.AllDirectories)
                .Where(x => x
                    .Split(Path.DirectorySeparatorChar)
                    .Count() - 2 == dirDepth)
                .ToList();

            var groupedFiles = allFiles
                .GroupBy(x => x
                    .Split(Path.DirectorySeparatorChar)
                    .Skip(dirDepth)
                    .First(), y => y)
                .ToDictionary(x => x.Key, y => y.ToList());

            var metaRegex = new Regex("[.]meta[.]json$", RegexOptions.IgnoreCase);
            var defaultTexMeta = TexMeta.DefaultFor(state.SystemInfo.Platform);

            var miloDir = new MiloObjectDir();
            var miloTypes = groupedFiles.Keys.ToList();

            if (state.SystemInfo.Version >= 24)
            {
                // Find directory entry
                var dirType = directoryTypes
                    .Intersect(miloTypes)
                    .Single();

                var dirEntryPath = groupedFiles[dirType]
                    .Single();

                var dirEntry = new MiloObjectBytes(dirType)
                {
                    Name = Path.GetFileName(dirEntryPath),
                    Data = File.ReadAllBytes(dirEntryPath)
                };

                miloDir.Extras.Add("DirectoryEntry", dirEntry);
                miloDir.Extras.Add("Num1", 0);
                miloDir.Extras.Add("Num2", 0);

                miloTypes.Remove(dirType);
            }

            foreach (var type in miloTypes)
            {
                var metaPaths = groupedFiles[type]
                    .Where(x => metaRegex.IsMatch(x))
                    .ToList();

                var filePaths = groupedFiles[type]
                    .Except(metaPaths)
                    .ToList();

                /*
                if (type == "Tex")
                {
                    var texMeta = metaPaths
                        .ToDictionary(x => x, y => JsonSerializer.Deserialize<TexMeta>(File.ReadAllText(y), state.JsonSerializerOptions));



                    continue;
                }*/

                foreach (var entryPath in filePaths)
                {
                    var entry = new MiloObjectBytes(type)
                    {
                        Name = Path.GetFileName(entryPath),
                        Data = File.ReadAllBytes(entryPath)
                    };

                    miloDir.Entries.Add(entry);
                }
            }

            var serializer = state.GetSerializer();

            var miloFile = new MiloFile();
            miloFile.Data = serializer.WriteToBytes(miloDir);


            miloFile.WriteToFile(outputPath);
        }

        public static MiloSerializer GetSerializer(this AppState state) => new MiloSerializer(state.SystemInfo);
    }
}
