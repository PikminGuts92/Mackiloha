using System;
using System.Collections.Generic;
using System.Text;

namespace P9SongTool.Models
{
    public class SongPreferences
    {
        public string Venue { get; set; }
        public List<string> MiniVenues { get; set; }
        public List<string> Scenes { get; set; }

        public string StudioOutfit { get; set; }
        public string DreamscapeOutfit { get; set; }

        public List<string> GeorgeInstruments { get; set; }
        public List<string> JohnInstruments { get; set; }
        public List<string> PaulInstruments { get; set; }
        public List<string> RingoInstruments { get; set; }

        public string Tempo { get; set; }
        public string SongClips { get; set; }
        public string DreamscapeFont { get; set; }

        public string GeorgeAmp { get; set; }
        public string JohnAmp { get; set; }
        public string PaulAmp { get; set; }
        public string Mixer { get; set; }
        public string DreamscapeCamera { get; set; }

        public string LyricPart { get; set; }
    }
}
