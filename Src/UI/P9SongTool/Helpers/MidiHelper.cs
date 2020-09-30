using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace P9SongTool.Helpers
{
    public class MidiHelper
    {
        protected readonly List<(long tickPos, decimal framePos, int mpq)> TempoChanges;
        protected readonly decimal Framerate = 30.0M;
        protected readonly MidiFile BaseMidi;

        public MidiHelper(string midPath)
        {
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
            var ticksPerQuarter = GetTicksPerQuarter();

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

        internal long FramePosToTicks(decimal framePos)
        {
            // Some positions are negative
            if (framePos <= 0.0M)
                return 0L;

            var ticksPerQuarter = GetTicksPerQuarter();

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
            return currentTempo.tickPos + deltaTicks;
        }

        internal List<List<MidiEvent>> CreateMidiTracksFromBase()
        {
            var midiTracks = new List<List<MidiEvent>>();

            if (!(BaseMidi is null))
            {
                // Copy tracks
                foreach (var baseTrack in BaseMidi.Events)
                {
                    // Shallow copy events
                    var track = new List<MidiEvent>();
                    track.AddRange(baseTrack);

                    midiTracks.Add(track);
                }
            }
            else
            {
                // Create basic tempo track
                var tempoTrack = new List<MidiEvent>();
                tempoTrack.Add(new TextEvent("animTempo", MetaEventType.SequenceTrackName, 0));
                tempoTrack.Add(new MetaEvent(MetaEventType.EndTrack, 0, 0));

                midiTracks.Add(tempoTrack);
            }

            return midiTracks;
        }

        internal int GetTicksPerQuarter()
            => !(BaseMidi is null)
                ? BaseMidi.DeltaTicksPerQuarterNote
                : 480;
    }
}
