using Mackiloha;
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

            var trackNames = new[] { "VENUE" }
                .Concat(TBRBCharacters.Select(x => x.ToUpper()));

            // Parse venue track
            foreach (var trackName in trackNames)
            {
                // Check if track exists
                if (!midTracks.TryGetValue(trackName, out var track))
                    continue;

                // Get midi text events
                var venueEvents = track
                    .Where(x => (x is TextEvent te)
                            && te.MetaEventType == MetaEventType.TextEvent)
                    .OrderBy(x => x.AbsoluteTime) // Should already be sort (but just in case)
                    .Select(x => (MidiHelper.TickPosToFrames(x.AbsoluteTime), (x as TextEvent).Text))
                    .ToList();

                var isCharTrack = trackName != "VENUE";
                var appendName = isCharTrack
                    ? $"_{trackName.ToLower()}"
                    : "";

                // Parse text events
                foreach (var (pos, text) in venueEvents)
                {
                    // If not formatted as anim event, skip
                    if (!IsPropEvent(text))
                        continue;
                    
                    // Parse event name and values from text
                    var (propEvName, propEvValues) = ParseEventValues(text, isCharTrack, appendName);

                    // Get director group
                    if (!directedGroups.TryGetValue(propEvName, out var dirGroup))
                    {
                        // Not found, create dir group
                        dirGroup = CreateDirectorGroup(propEvName);
                        directedGroups.Add(propEvName, dirGroup);
                    }

                    // Convert string array of values to anim event
                    var ev = GetDirectedEvent((float)pos, propEvValues, dirGroup.EventType);
                    dirGroup.Events.Add(ev);
                }
            }

            var eventGroups = directedGroups
                .Select(x => x.Value)
                .ToList();

            var totalTime = eventGroups
                .SelectMany(x => x.Events)
                .Select(x => x.Position)
                .Max();

            var anim = new PropAnim()
            {
                Name = "song.anim",
                AnimName = "song_anim",
                TotalTime = totalTime
            };

            anim.DirectorGroups.AddRange(eventGroups);
            return anim;
        }

        protected bool IsPropEvent(string text)
            => PropNameRegex.IsMatch(text);

        protected (string name, string[] values) ParseEventValues(string text, bool isChar, string appendName)
        {
            var match = PropNameRegex.Match(text);
            if (!match.Success)
                throw new Exception("Event text is not valid prop event syntax");

            string name;
            string[] values;

            if (match.Groups[3].Success)
            {
                name = match.Groups[1].Value.ToLower();

                if (!isChar &&name.Equals("config", StringComparison.CurrentCultureIgnoreCase))
                    name = "configuration";

                values = match.Groups[3].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                values = new[] { match.Groups[1].Value };

                if (isChar)
                    name = "body";
                else if (values.Any(x => x.EndsWith(".pp")))
                    name = "postproc";
                else if (values.Any(x => x.EndsWith(".anim")))
                    name = "lyric_transition";
                else
                    name = "shot";
            }

            if (!string.IsNullOrWhiteSpace(appendName))
                name += appendName;

            return (name, values);
        }

        protected DirectedEventGroup CreateDirectorGroup(string propName)
            => new DirectedEventGroup()
            {
                EventType = propName switch
                {
                    var p when p.StartsWith("postproc") => DirectedEventType.TextFloat,
                    var p when p.StartsWith("lookat") => DirectedEventType.Float,
                    var p when p.StartsWith("face_weight") => DirectedEventType.Float,
                    var p when p.StartsWith("sing") => DirectedEventType.Float,
                    _ => DirectedEventType.Text
                },
                DirectorName = "P9Director",
                PropName = propName,
                Unknown1 = propName switch
                {
                    "rotation" => 1,
                    "scale" => 1,
                    "position" => 1,
                    var p when p.StartsWith("face_weight") => 1,
                    var p when p.StartsWith("sing") => 2,
                    var p when p.StartsWith("lookat") => 4,
                    _ => 0
                },
                PropName2 = propName switch
                {
                    "postproc" => "postproc_interp",
                    "hist_lightpreset" => "hist_lightpreset_interp",
                    _ => ""
                },
                Unknown2 = propName switch
                {
                    "rotation" => 1,
                    "scale" => 2,
                    "position" => 3,
                    "postproc" => 5,
                    "hist_lightpreset" => 5,
                    "configuration" => 6,
                    _ => 0
                },
                Events = new List<IDirectedEvent>()
            };

        protected IDirectedEvent GetDirectedEvent(float pos, string[] values, DirectedEventType evType)
        {
            if (evType == DirectedEventType.Float)
            {
                float.TryParse(values.FirstOrDefault(), out var fValue);

                return new DirectedEventFloat()
                {
                    Position = pos,
                    Value = fValue
                };
            }
            else if (evType == DirectedEventType.TextFloat)
            {
                float.TryParse(values.Skip(1).FirstOrDefault(), out var fValue);

                return new DirectedEventTextFloat()
                {
                    Position = pos,
                    Text = values.FirstOrDefault() ?? "",
                    Value = fValue
                };
            }
            else if (evType == DirectedEventType.Boolean)
            {
                bool.TryParse(values.FirstOrDefault()?.ToLower(), out var bValue);

                return new DirectedEventBoolean()
                {
                    Position = pos,
                    Enabled = bValue
                };
            }
            else if (evType == DirectedEventType.Vector4)
            {
                float.TryParse(values.FirstOrDefault()?.ToLower(), out var v1);
                float.TryParse(values.Skip(1).FirstOrDefault()?.ToLower(), out var v2);
                float.TryParse(values.Skip(2).FirstOrDefault()?.ToLower(), out var v3);
                float.TryParse(values.Skip(3).FirstOrDefault()?.ToLower(), out var v4);

                return new DirectedEventVector4()
                {
                    Position = pos,
                    Value = new Vector4()
                    {
                        X = v1,
                        Y = v2,
                        Z = v3,
                        W = v4
                    }
                };
            }
            else if (evType == DirectedEventType.Vector3)
            {
                float.TryParse(values.FirstOrDefault()?.ToLower(), out var v1);
                float.TryParse(values.Skip(1).FirstOrDefault()?.ToLower(), out var v2);
                float.TryParse(values.Skip(2).FirstOrDefault()?.ToLower(), out var v3);

                return new DirectedEventVector3()
                {
                    Position = pos,
                    Value = new Vector3()
                    {
                        X = v1,
                        Y = v2,
                        Z = v3
                    }
                };
            }
            else // DirectedEventType.Text
            {
                return new DirectedEventText()
                {
                    Position = pos,
                    Text = values.FirstOrDefault() ?? ""
                };
            }
        }
    }
}
