using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Song;
using P9SongTool.Exceptions;
using P9SongTool.Helpers;
using P9SongTool.Json;
using P9SongTool.Models;
using P9SongTool.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace P9SongTool.Apps
{
    public class Milo2ProjectApp
    {
        public void Parse(Milo2ProjectOptions op)
        {
            var appState = AppState.FromFile(op.InputPath);
            appState.UpdateSystemInfo(GetSystemInfo(op));

            var inputMiloPath = Path.GetFullPath(op.InputPath); // Use abs path until AppState is updated
            var milo = appState.OpenMiloFile(inputMiloPath);

            // Create output directory
            var outputDir = Path.GetFullPath(op.OutputPath);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var entries = GetEntries(milo);
            ExtractLipsync(entries, outputDir);

            var serializer = appState.GetSerializer();

            var propAnims = entries
                .Where(x => x.Type == "PropAnim")
                .Select(y => serializer.ReadFromMiloObjectBytes<PropAnim>(y as MiloObjectBytes))
                .ToList();

            var songPref = entries
                .Where(x => x.Type == "P9SongPref")
                .Select(y => serializer.ReadFromMiloObjectBytes<P9SongPref>(y as MiloObjectBytes))
                .FirstOrDefault();

            if (songPref is null)
                throw new UnsupportedMiloException("No P9SongPref entry was found");

            // Write song json
            var song = new P9Song()
            {
                Name = milo.Name,
                Preferences = ConvertFromP9SongPref(songPref)
            };

            // Convert lyric config
            var lyricConfig = propAnims
                .FirstOrDefault(x => x.Name == "lyric_config.anim");

            if (!(lyricConfig is null))
            {
                song.LyricConfigurations = ConvertFromPropAnim(lyricConfig);
            }

            var songJson = SerializeSong(song, appState);
            var songJsonPath = Path.Combine(outputDir, "song.json");

            File.WriteAllText(songJsonPath, songJson);
            Console.WriteLine($"Wrote \"song.json\"");

            // Export midi
            var songAnim = propAnims
                .First(x => x.Name == "song.anim");

            var converter = new Anim2Midi(songAnim, op.BaseMidiPath);
            converter.ExportMidi(Path.Combine(outputDir, "venue.mid"));
            Console.WriteLine($"Wrote \"venue.mid\"");

            // Export whatever remaining files
            var remaining = entries
                .Where(x => x.Name != songAnim.Name
                    && x.Type != "CharLipSync"
                    && x.Type != "P9SongPref"
                    && x.Name != "lyric_config.anim")
                .ToList();

            var extraDirPath = Path.Combine(outputDir, "extra");
            if (!Directory.Exists(extraDirPath))
                Directory.CreateDirectory(extraDirPath);

            foreach (var entry in remaining)
            {
                var entryPath = Path.Combine(extraDirPath, entry.Name);
                var miloObj = entry as MiloObjectBytes;

                File.WriteAllBytes(entryPath, miloObj.Data);
                Console.WriteLine($"Extracted \"{miloObj.Name}\"");
            }

            Console.WriteLine($"Successfully created project in \"{outputDir}\"");
        }

        protected SystemInfo GetSystemInfo(Milo2ProjectOptions op)
            => new SystemInfo()
            {
                Version = 25,
                BigEndian = true,
                Platform = op.InputPath
                    .ToLower()
                    .EndsWith("_ps3")
                    ? Platform.PS3
                    : Platform.X360
            };

        protected List<MiloObject> GetEntries(MiloObjectDir miloDir)
        {
            var entries = new List<MiloObject>();
            GetEntries(miloDir, entries);
            return entries;
        }

        protected void GetEntries(MiloObjectDir miloDir, List<MiloObject> entries)
        {
            if (miloDir.Type != "ObjectDir")
                throw new UnsupportedMiloException($"Directory type of \"{miloDir.Type}\" found, expected \"ObjectDir\" expected");

            // Traverse sub directories
            if (!(miloDir.GetDirectoryEntry() is MiloObjectDirEntry dirEntry))
                throw new UnsupportedMiloException("Could not parse directory entry");

            foreach (var subDir in dirEntry.SubDirectories)
            {
                GetEntries(subDir, entries);
            }

            // Add entries in current directory
            entries.AddRange(miloDir.Entries);
        }

        protected void ExtractLipsync(List<MiloObject> entries, string dirPath)
        {
            var lipsyncEntries = entries
                .Where(x => x.Type == "CharLipSync")
                .ToList();

            var lipDirPath = Path.Combine(dirPath, "lipsync");
            if (!Directory.Exists(lipDirPath))
                Directory.CreateDirectory(lipDirPath);

            foreach (var lipsync in lipsyncEntries)
            {
                var lipFilePath = Path.Combine(lipDirPath, lipsync.Name);

                var lipBytes = lipsync as MiloObjectBytes; // Should always be this
                File.WriteAllBytes(lipFilePath, lipBytes.Data);

                Console.WriteLine($"Extracted \"{lipsync.Name}\"");
            }
        }

        protected SongPreferences ConvertFromP9SongPref(P9SongPref songPref)
            => new SongPreferences()
            {
                Venue = songPref.Venue,
                MiniVenues = songPref.MiniVenues.ToList(),
                Scenes = songPref.Scenes.ToList(),

                DreamscapeOutfit = songPref.DreamscapeOutfit,
                StudioOutfit = songPref.StudioOutfit,

                GeorgeInstruments = songPref.GeorgeInstruments.ToList(),
                JohnInstruments = songPref.JohnInstruments.ToList(),
                PaulInstruments = songPref.PaulInstruments.ToList(),
                RingoInstruments = songPref.RingoInstruments.ToList(),

                Tempo = songPref.Tempo,
                SongClips = songPref.SongClips,
                DreamscapeFont = songPref.DreamscapeFont,

                // TBRB specific
                GeorgeAmp = songPref.GeorgeAmp,
                JohnAmp = songPref.JohnAmp,
                PaulAmp = songPref.PaulAmp,
                Mixer = songPref.Mixer,
                DreamscapeCamera = Enum.GetName(typeof (DreamscapeCamera), songPref.DreamscapeCamera),

                LyricPart = songPref.LyricPart
            };

        protected LyricConfig[] ConvertFromPropAnim(PropAnim lyricConfigProp)
        {
            var groupedConfigs = lyricConfigProp
                .DirectorGroups
                .GroupBy(x => x.DirectorName)
                .OrderBy(x => x.Key);

            var lyricConfigs = new List<LyricConfig>();

            foreach (var propConfig in groupedConfigs)
            {
                var parts = propConfig
                    .OrderBy(x => x.DirectorName)
                    .ToList();

                var eventCount = parts
                    .Select(x => x.Events.Count)
                    .Max();

                var lyricEvents = new List<LyricEvent>();

                foreach (var i in Enumerable.Range(0, eventCount))
                {
                    var pos = (DirectedEventVector3)parts[0].Events[i];
                    var rot = (DirectedEventVector4)parts[1].Events[i];
                    var scale = (DirectedEventVector3)parts[2].Events[i];

                    lyricEvents.Add(new LyricEvent()
                    {
                        Time = pos.Position, // For now assume positions match between pos, rot, and scale
                        Position = new float[]
                        {
                            pos.Value.X,
                            pos.Value.Y,
                            pos.Value.Z
                        },
                        Rotation = new float[]
                        {
                            rot.Value.X,
                            rot.Value.Y,
                            rot.Value.Z,
                            rot.Value.W
                        },
                        Scale = new float[]
                        {
                            scale.Value.X,
                            scale.Value.Y,
                            scale.Value.Z
                        }
                    });
                }

                lyricConfigs.Add(new LyricConfig()
                {
                    Name = parts.First().DirectorName,
                    Events = lyricEvents
                        .ToArray()
                }); ;
            }

            return lyricConfigs
                .ToArray();
        }

        public string SerializeSong(P9Song song, AppState appState)
        {
            var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings();
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            jsonSettings.Converters.Add(new SingleLineFloatArrayConverter());

            return Newtonsoft.Json.JsonConvert.SerializeObject(song, jsonSettings);
            //return JsonSerializer.Serialize(song, appState.JsonSerializerOptions);
        }
    }
}
