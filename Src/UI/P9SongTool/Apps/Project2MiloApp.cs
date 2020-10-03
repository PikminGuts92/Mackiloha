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

            // Add preference + anim
            miloDir.Entries.Add(songPref);
            miloDir.Entries.Add(anim);

            // TODO: Add extras

            var serializer = state.GetSerializer();

            var miloFile = new MiloFile
            {
                Data = serializer.WriteToBytes(miloDir)
            };

            miloFile.Structure = BlockStructure.MILO_A;
            miloFile.WriteToFile(op.OutputPath);
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

            Enum.TryParse<DreamscapeCamera>(preferences.DreamscapeCamera, out var dreamCam);
            songPref.DreamscapeCamera = dreamCam;

            songPref.LyricPart = preferences.LyricPart;

            return songPref;
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
