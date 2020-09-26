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
    public class Milo2ProjectApp
    {
        protected readonly string[] Characters = new[] { "john", "george", "paul", "ringo"  };

        public void Parse(Milo2ProjectOptions op)
        {
            var appState = AppState.FromFile(op.InputPath);
            appState.UpdateSystemInfo(GetSystemInfo(op));

            var milo = appState.OpenMiloFile(op.InputPath);

            // Create output directory
            if (!Directory.Exists(op.OutputPath))
                Directory.CreateDirectory(op.OutputPath);

            var entries = GetEntries(milo);
            ExtractLipsync(entries, op.OutputPath);

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

            var songJson = JsonSerializer.Serialize(song, appState.JsonSerializerOptions);
            var songJsonPath = Path.Combine(op.OutputPath, "song.json");

            File.WriteAllText(songJsonPath, songJson);

            // Export midi
            var songAnim = propAnims.First(x => x.Name == "song.anim");

            var converter = new Anim2Midi(songAnim, op.BaseMidiPath);
            converter.ExportMidi(Path.Combine(op.OutputPath, "venue.mid"));
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

            foreach (var lipsync in lipsyncEntries)
            {
                var lipFilePath = Path.Combine(dirPath, lipsync.Name);

                var lipBytes = lipsync as MiloObjectBytes; // Should always be this
                File.WriteAllBytes(lipFilePath, lipBytes.Data);
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
    }
}
