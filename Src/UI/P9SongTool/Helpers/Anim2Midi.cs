using Mackiloha.Song;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace P9SongTool.Helpers
{
    public class Anim2Midi
    {
        protected readonly decimal Framerate = 30.0M;
        protected readonly string[] TBRBCharacters = new[] { "paul", "john", "george", "ringo" };

        protected readonly PropAnim Anim;
        protected readonly MidiFile BaseMidi;

        protected readonly List<(long tickPos, decimal framePos, int mpq)> TempoChanges;

        public Anim2Midi(PropAnim anim, string midPath)
        {
            Anim = anim;
            BaseMidi = ParseBaseMidi(midPath);
            TempoChanges = CreateTempoMap();
        }

        protected virtual MidiFile ParseBaseMidi(string midPath)
        {
            // Check if mid exists
            if ((midPath is null) || !File.Exists(midPath))
            {
                // No mid found
                return null;
            }

            return new MidiFile(midPath);
        }

        protected virtual List<(long tickPos, decimal framePos, int mpq)> CreateTempoMap()
        {
            var calculatedTempos = new List<(long tickPos, decimal framePos, int mpq)>();
            var mid = this.BaseMidi;

            var fps = Framerate;
            var ticksPerQuarter = mid.DeltaTicksPerQuarterNote;

            var currentTickPos = 0L;
            var currentFramePos = 0.0M;
            var currentMpq = 60_000_000 / 120;

            if (mid is null)
            {
                // No mid found, return default
                calculatedTempos.Add((currentTickPos, currentFramePos, currentMpq));
                return calculatedTempos;
            }

            var tempoChanges = mid.Events
                .First()
                .Where(x => x is TempoEvent)
                .Select(x => x as TempoEvent)
                .OrderBy(x => x.AbsoluteTime)
                .ToList();

            if (tempoChanges.Count <= 0)
            {
                // No tempo events found, return default
                calculatedTempos.Add((currentTickPos, currentFramePos, currentMpq));
                return calculatedTempos;
            }

            foreach (var tempo in tempoChanges)
            {
                var deltaTicks = tempo.AbsoluteTime - currentTickPos;
                var deltaFrames = (Framerate * deltaTicks * currentMpq) / (1_000_000 * ticksPerQuarter);

                // Set current tempo values
                currentTickPos = tempo.AbsoluteTime;
                currentFramePos += deltaFrames;
                currentMpq = tempo.MicrosecondsPerQuarterNote;

                // Add tempo
                calculatedTempos.Add((currentTickPos, currentFramePos, currentMpq));
            }

            return calculatedTempos;
        }

        public void ExportMidi(string exportMidPath)
        {
            // Create directory if it doesn't exist
            var dirPath = Path.GetDirectoryName(exportMidPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var mid = new MidiEventCollection(1, 480);

            // Create dictionary of tracks to filter events in
            var midFilteredTracks = TBRBCharacters
                .Select(x => x.ToUpper())
                .Concat(new[] { "VENUE" })
                .ToDictionary(x => x, y => new List<MidiEvent>());

            var nameFilterRegex = new Regex($"(?i)([_]?)({string.Join("|", TBRBCharacters)})$");

            // Add basic tempo track
            var tempoTrack = new List<MidiEvent>();

            if (!(BaseMidi is null))
            {
                // Use existing tempo track
                tempoTrack.AddRange(BaseMidi.Events.First());
            }
            else
            {
                // Create basic tempo track
                tempoTrack.Add(new TextEvent("animTempo", MetaEventType.SequenceTrackName, 0));
                tempoTrack.Add(new MetaEvent(MetaEventType.EndTrack, 0, 0));
            }
            mid.AddTrack(tempoTrack);

            foreach (var group in Anim.DirectorGroups)
            {
                // Default event name + track
                var eventName = group.PropName;
                var track = midFilteredTracks["VENUE"];

                var match = nameFilterRegex.Match(group.PropName);
                if (match.Success)
                {
                    // Get last group match (ex: "_ringo" -> "ringo" and "spot_paul" -> "paul")
                    var key = match
                        .Groups
                        .Values
                        .Last()
                        .Value
                        .ToUpper();

                    // Look for character track
                    if (midFilteredTracks.ContainsKey(key))
                    {
                        // Remove appended name + assign character track
                        eventName = nameFilterRegex.Replace(eventName, "");
                        track = midFilteredTracks[key];
                    }
                }

                foreach (var ev in group.Events.OrderBy(x => x.Position))
                {
                    var tickPos = FramePosToTicks((decimal)ev.Position);

                    var evValue = ev switch
                    {
                        DirectedEventFloat evFloat => $"{evFloat.Value}",
                        DirectedEventTextFloat { Value: 0.0f } evTextFloat => $"{evTextFloat.Text}", // postproc events
                        DirectedEventTextFloat evTextFloat => $"{evTextFloat.Text} {evTextFloat.Value}",
                        DirectedEventBoolean evBool => $"{(evBool.Enabled ? "TRUE" : "FALSE")}",
                        DirectedEventVector4 evVector4 => $"{evVector4.Value.X} {evVector4.Value.Y} {evVector4.Value.Z} {evVector4.Value.W}",
                        DirectedEventVector3 evVector3 => $"{evVector3.Value.X} {evVector3.Value.Y} {evVector3.Value.Z}",
                        DirectedEventText evTextFloat => $"{evTextFloat.Text}",
                        _ => throw new NotSupportedException()
                    };

                    var evText = eventName switch
                    {
                        "body" => $"[{evValue}]",
                        "shot" => $"[{evValue}]",
                        "configuration" => $"[config ({evValue})]", // Use alias
                        "postproc" when evValue.EndsWith(".pp") && !evValue.Contains(' ') => $"[{evValue}]",
                        "lyric_transition" when evValue.EndsWith(".anim") && !evValue.StartsWith('(') => $"[{evValue}]",
                        _ => $"[{eventName} ({evValue})]",
                    };
                    track.Add(new TextEvent(evText, MetaEventType.TextEvent, tickPos));
                }
            }

            foreach (var key in midFilteredTracks.Keys)
            {
                if (midFilteredTracks[key].Count <= 0)
                {
                    // Remove empty track
                    midFilteredTracks.Remove(key);
                }
            }

            foreach (var kv in midFilteredTracks)
            {
                var trackName = kv.Key;
                var track = kv.Value;

                // Sort events
                track.Sort((x, y) => x.AbsoluteTime.CompareTo(y.AbsoluteTime));

                // Insert track name
                track.Insert(0, new TextEvent(trackName, MetaEventType.SequenceTrackName, 0));

                // Add end event
                track.Add(new MetaEvent(MetaEventType.EndTrack, 0, track.Max(x => x.AbsoluteTime)));
                mid.AddTrack(track);
            }

            MidiFile.Export(exportMidPath, mid);
        }

        protected long FramePosToTicks(decimal framePos)
        {
            // Some position are negative
            if (framePos <= 0.0M)
                return 0L;

            var ticksPerQuarter = !(BaseMidi is null)
                ? BaseMidi.DeltaTicksPerQuarterNote
                : 480;

            var currentTempo = TempoChanges.First();

            foreach (var change in TempoChanges.Skip(1))
            {
                if (change.framePos > (decimal)framePos)
                    break;

                currentTempo = change;
            }

            var mpq = currentTempo.mpq;
            var fps = Framerate;

            var deltaPos = framePos - currentTempo.framePos;
            var seconds = deltaPos / fps;

            long deltaTicks = (1000L * (long)(seconds * 1000) * ticksPerQuarter) / mpq;
            return currentTempo.tickPos +  deltaTicks;
        }
    }
}
