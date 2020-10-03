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
    }
}
