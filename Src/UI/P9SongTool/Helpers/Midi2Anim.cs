using Mackiloha.Song;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace P9SongTool.Helpers
{
    public class Midi2Anim
    {
        protected readonly string[] TBRBCharacters = new[] { "paul", "john", "george", "ringo" };
        protected readonly Regex PropNameRegex = new Regex(@"(?i)^\[([^\s]+)(\s+\(([^\)]*)\))?\]$");

        protected readonly MidiHelper MidiHelper;

        public Midi2Anim(string midPath)
        {
            MidiHelper = new MidiHelper(midPath);
        }

        public PropAnim ExportToAnim()
        {
            var midTracks = MidiHelper.CreateMidiTracksDictionaryFromBase();

            var directedGroups = new Dictionary<string, DirectedEventGroup>();

            // Parse venue track
            if (midTracks.TryGetValue("VENUE", out var venueTrack))
            {
                var venueEvents = venueTrack
                    .Where(x => (x is TextEvent te)
                            && te.MetaEventType == MetaEventType.TextEvent)
                    .Select(x => (MidiHelper.TickPosToFrames(x.AbsoluteTime), (x as TextEvent).Text))
                    .ToList();


                foreach (var ev in venueEvents)
                {
                    var result2 = ParseEventValues(ev.Text, out var result);
                }
            }

            var midTrackNames = TBRBCharacters
                .Select(x => x.ToUpper())
                .ToList();

            // Parse character tracks
            foreach (var trackName in midTrackNames)
            {
                // Check if track exists
                if (!midTracks.TryGetValue(trackName, out var track))
                    continue;


            }

            return new PropAnim()
            {
                Name = "song.anim",
                AnimName = "song_anim",
                TotalTime = 0.0f // Get from last note
            };
        }

        public bool ParseEventValues(string text, out (string name, string[] values) nameValues)
        {
            var match = PropNameRegex.Match(text);
            if (!match.Success)
            {
                nameValues = default;
                return false;
            }

            string name;
            string[] values;

            if (match.Groups[3].Success)
            {
                name = match.Groups[1].Value;
                values = match.Groups[3].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                name = "shot";
                values = new[] { match.Groups[1].Value };
            }

            nameValues = (name, values);
            return true;
        }
    }
}
