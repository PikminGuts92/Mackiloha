using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Song;
using P9SongTool.Exceptions;
using P9SongTool.Helpers;
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
    public class Project2MiloApp
    {
        protected readonly (string extension, string miloType)[] SupportedExtraTypes;

        public Project2MiloApp()
        {
            SupportedExtraTypes = new (string extension, string miloType)[]
            {
                (".anim", "PropAnim"),
                (".mat", "Mat"),
                (".tex", "Tex")
            };
        }

        public void Parse(Project2MiloOptions op)
        {
            if (!Directory.Exists(op.InputPath))
                throw new MiloBuildException("Input directory doesn't exist");

            var inputDir = Path.GetFullPath(op.InputPath);
            var songMetaPath = Path.Combine(inputDir, "song.json");
            var midPath = Path.Combine(inputDir, "venue.mid");

            var lipsyncPaths = Directory.GetFiles(Path.Combine(inputDir, "lipsync"), "*.lipsync");
            var extrasPaths = Directory.GetFiles(Path.Combine(inputDir, "extra"));

            // Enforce files exist
            if (!File.Exists(songMetaPath))
                throw new MiloBuildException("Can't find \"song.json\" file");
            if (!File.Exists(midPath))
                throw new MiloBuildException("Can't find \"venue.mid\" file");

            var p9song = OpenP9File(songMetaPath);
            var songPref = ConvertFromSongPreferences(p9song.Preferences);
            var lyricConfigAnim = ConvertFromLyricConfigs(p9song.LyricConfigurations);

            var converter = new Midi2Anim(midPath);
            var anim = converter.ExportToAnim();

            // Create app state
            var state = new AppState(inputDir);
            state.UpdateSystemInfo(GetSystemInfo(op));

            var miloDir = CreateRootDirectory(p9song.Name?.ToLower());
            var miloDirEntry = miloDir.Extras["DirectoryEntry"] as MiloObjectDirEntry;

            // Add lipsync files
            foreach (var lipPath in lipsyncPaths)
            {
                var subDir = GetCharSubDirectory(lipPath);
                miloDirEntry.SubDirectories.Add(subDir);
            }

            // Add preference + anims
            miloDir.Entries.Add(songPref);
            miloDir.Entries.Add(anim);

            if (!(lyricConfigAnim is null))
            {
                miloDir.Entries.Add(lyricConfigAnim);
            }

            // Get extras as raw entries
            var extras = extrasPaths
                .Select(x => (x, SupportedExtraTypes.FirstOrDefault(y => x.EndsWith(y.extension, StringComparison.CurrentCultureIgnoreCase)).miloType))
                .Where(x => !(x.Item1 is null))
                .Select(x => new MiloObjectBytes(x.Item2)
                {
                    Name = Path.GetFileName(x.Item1),
                    Data = File.ReadAllBytes(x.Item1)
                })
                .Where(x => (lyricConfigAnim is null)
                    || !x.Name.Equals(lyricConfigAnim.Name, StringComparison.CurrentCultureIgnoreCase)) // Filter out lyric_config if in json
                .ToList();

            miloDir.Entries.AddRange(extras);

            var serializer = state.GetSerializer();
            var outputMiloPath = Path.GetFullPath(op.OutputPath);

            var miloFile = new MiloFile
            {
                Data = serializer.WriteToBytes(miloDir)
            };

            miloFile.Structure = op.UncompressedMilo
                ? BlockStructure.MILO_A
                : BlockStructure.MILO_B;

            miloFile.WriteToFile(op.OutputPath);
            Console.WriteLine($"Successfully created milo at \"{outputMiloPath}\"");
        }

        protected P9Song OpenP9File(string p9songPath)
        {
            var appState = AppState.FromFile(p9songPath);
            var jsonText = File.ReadAllText(p9songPath);

            return JsonSerializer.Deserialize<P9Song>(jsonText, appState.JsonSerializerOptions);
        }

        protected P9SongPref ConvertFromSongPreferences(SongPreferences preferences)
        {
            var songPref = new P9SongPref();

            songPref.Name = "P9SongPref";
            songPref.Venue = preferences.Venue;
            songPref.MiniVenues.AddRange(preferences.MiniVenues);
            songPref.Scenes.AddRange(preferences.Scenes);
            
            songPref.DreamscapeOutfit = preferences.DreamscapeOutfit;
            songPref.StudioOutfit = preferences.StudioOutfit;
            
            songPref.GeorgeInstruments.AddRange(preferences.GeorgeInstruments);
            songPref.JohnInstruments.AddRange(preferences.JohnInstruments);
            songPref.PaulInstruments.AddRange(preferences.PaulInstruments);
            songPref.RingoInstruments.AddRange(preferences.RingoInstruments);
            
            songPref.Tempo = preferences.Tempo;
            songPref.SongClips = preferences.SongClips;
            songPref.DreamscapeFont = preferences.DreamscapeFont;
            
            // TBRB specific
            songPref.GeorgeAmp = preferences.GeorgeAmp;
            songPref.JohnAmp = preferences.JohnAmp;
            songPref.PaulAmp = preferences.PaulAmp;
            songPref.Mixer = preferences.Mixer;

            var camera = preferences.DreamscapeCamera;
            if (!(camera is null)
                && (camera != "None")
                && !camera.StartsWith("kP9"))
                camera = $"kP9{camera}"; // Prepend with prefix

            Enum.TryParse<DreamscapeCamera>(camera, out var dreamCam);
            songPref.DreamscapeCamera = dreamCam;

            songPref.LyricPart = preferences.LyricPart;

            return songPref;
        }

        protected PropAnim ConvertFromLyricConfigs(LyricConfig[] lyricConfigs)
        {
            if (lyricConfigs is null || lyricConfigs.Length <= 0)
            {
                return default;
            }

            var lyricAnim = new PropAnim()
            {
                Name = "lyric_config.anim",
                AnimName = "",
                TotalTime = lyricConfigs
                    .Select(x => x.Events.Count())
                    .Max()
            };

            int i = 1;
            foreach (var lyricConfig in lyricConfigs.OrderBy(x => x.Name))
            {
                var name = lyricConfig?.Name ?? $"venue_lyric{i:d2}";

                // Create groups
                var posGroup = Midi2Anim.CreateDirectorGroup("position", name);
                var rotGroup = Midi2Anim.CreateDirectorGroup("rotation", name);
                var scaleGroup = Midi2Anim.CreateDirectorGroup("scale", name);

                // Convert position
                lyricConfig
                    .Events
                    .OrderBy(x => x.Time)
                    .Select(x => new DirectedEventVector3()
                    {
                        Position = x.Time,
                        Value = new Vector3()
                        {
                            X = (!(x.Position is null)
                                && x.Position.Length > 0)
                                ? x.Position[0]
                                : 0.0f,
                            Y = (!(x.Position is null)
                                && x.Position.Length > 1)
                                ? x.Position[1]
                                : 0.0f,
                            Z = (!(x.Position is null)
                                && x.Position.Length > 2)
                                ? x.Position[2]
                                : 0.0f,
                        }
                    })
                    .ToList()
                    .ForEach(x => posGroup.Events.Add(x)); // Must add one at a time because of generic conflict

                // Convert rotation
                lyricConfig
                    .Events
                    .OrderBy(x => x.Time)
                    .Select(x => new DirectedEventVector4()
                    {
                        Position = x.Time,
                        Value = new Vector4()
                        {
                            X = (!(x.Rotation is null)
                                && x.Rotation.Length > 0)
                                ? x.Rotation[0]
                                : 0.0f,
                            Y = (!(x.Rotation is null)
                                && x.Rotation.Length > 1)
                                ? x.Rotation[1]
                                : 0.0f,
                            Z = (!(x.Rotation is null)
                                && x.Rotation.Length > 2)
                                ? x.Rotation[2]
                                : 0.0f,
                            W = (!(x.Rotation is null)
                                && x.Rotation.Length > 3)
                                ? x.Rotation[3]
                                : 1.0f,
                        }
                    })
                    .ToList()
                    .ForEach(x => rotGroup.Events.Add(x)); // Must add one at a time because of generic conflict

                // Convert scale
                lyricConfig
                    .Events
                    .OrderBy(x => x.Time)
                    .Select(x => new DirectedEventVector3()
                    {
                        Position = x.Time,
                        Value = new Vector3()
                        {
                            X = (!(x.Scale is null)
                                && x.Scale.Length > 0)
                                ? x.Scale[0]
                                : 1.0f,
                            Y = (!(x.Scale is null)
                                && x.Scale.Length > 1)
                                ? x.Scale[1]
                                : 1.0f,
                            Z = (!(x.Scale is null)
                                && x.Scale.Length > 2)
                                ? x.Scale[2]
                                : 1.0f,
                        }
                    })
                    .ToList()
                    .ForEach(x => scaleGroup.Events.Add(x)); // Must add one at a time because of generic conflict

                // Add groups to lyric anim
                lyricAnim.DirectorGroups.Add(posGroup);
                lyricAnim.DirectorGroups.Add(rotGroup);
                lyricAnim.DirectorGroups.Add(scaleGroup);

                i++;
            }

            return lyricAnim;
        }

        protected SystemInfo GetSystemInfo(Project2MiloOptions op)
            => new SystemInfo()
            {
                Version = 25,
                BigEndian = true,
                Platform = op.OutputPath
                    .ToLower()
                    .EndsWith("_ps3")
                    ? Platform.PS3
                    : Platform.X360
            };

        protected MiloObjectDir CreateRootDirectory(string name)
        {
            var miloDirEntry = new MiloObjectDirEntry()
            {
                Name = name,
                Version = 22,
                SubVersion = 2,
                ProjectName = "song",
                ImportedMiloPaths = new[]
                {
                    "../../world/shared/camera.milo",
                    "../../world/shared/director.milo"
                },
                SubDirectories = new List<MiloObjectDir>()
            };

            var miloDir = new MiloObjectDir()
            {
                Name = name
            };

            miloDir.Extras.Add("DirectoryEntry", miloDirEntry);
            miloDir.Extras.Add("Num1", 0);
            miloDir.Extras.Add("Num2", 0);
            return miloDir;
        }
        
        protected MiloObjectDir GetCharSubDirectory(string lipPath)
        {
            var name = Path.GetFileNameWithoutExtension(lipPath).ToLower();
            var fileName = Path.GetFileName(lipPath).ToLower();
            var data = File.ReadAllBytes(lipPath);

            var lipsync = new MiloObjectBytes("CharLipSync")
            {
                Name = fileName,
                Data = data
            };

            var miloDirEntry = new MiloObjectDirEntry()
            {
                Name = name,
                Version = 22,
                SubVersion = 2,
                ProjectName = "",
                ImportedMiloPaths = Array.Empty<string>(),
                SubDirectories = new List<MiloObjectDir>()
            };

            var miloDir = new MiloObjectDir()
            {
                Name = name
            };

            miloDir.Entries.Add(lipsync);
            miloDir.Extras.Add("DirectoryEntry", miloDirEntry);
            miloDir.Extras.Add("Num1", 0);
            miloDir.Extras.Add("Num2", 0);
            return miloDir;
        }
    }
}
