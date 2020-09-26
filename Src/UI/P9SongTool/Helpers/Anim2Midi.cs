using Mackiloha.Song;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace P9SongTool.Helpers
{
    public class Anim2Midi
    {
        protected readonly PropAnim Anim;
        protected readonly MidiFile BaseMidi;

        public Anim2Midi(PropAnim anim, string midPath)
        {
            Anim = anim;
            BaseMidi = ParseBaseMidi(midPath);
        }

        protected virtual MidiFile ParseBaseMidi(string midPath)
        {
            // Base mid not found, setup default values
            if ((midPath is null) || !File.Exists(midPath))
            {
                SetDefaultMidiValues();
                return null;
            }

            var mid = new MidiFile(midPath);

            return mid;
        }

        protected virtual void SetDefaultMidiValues()
        {

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

            // Add basic tempo track
            var tempoTrack = new List<MidiEvent>();
            tempoTrack.Add(new TextEvent("animTempo", MetaEventType.SequenceTrackName, 0));
            tempoTrack.Add(new MetaEvent(MetaEventType.EndTrack, 0, 0));
            mid.AddTrack(tempoTrack);

            foreach (var group in Anim.DirectorGroups)
            {
                var track = new List<MidiEvent>();
                track.Add(new TextEvent(group.PropName.ToUpper(), MetaEventType.SequenceTrackName, 0));

                foreach (var ev in group.Events.OrderBy(x => x.Position))
                {
                    var tickPos = FramePosToTicks(ev.Position);

                    var text = ev switch
                    {
                        DirectedEventFloat evFloat => $"[{evFloat.Value}]",
                        DirectedEventTextFloat evTextFloat => $"[{evTextFloat.Text} {evTextFloat.Value}]",
                        DirectedEventBoolean evBool => $"[{(evBool.Enabled ? "TRUE" : "FALSE")}]",
                        DirectedEventVector4 evVector4 => $"[{evVector4.Value.X} {evVector4.Value.Y} {evVector4.Value.Z} {evVector4.Value.W}]",
                        DirectedEventVector3 evVector3 => $"[{evVector3.Value.X} {evVector3.Value.Y} {evVector3.Value.Z}]",
                        DirectedEventText evTextFloat => $"[{evTextFloat.Text}]",
                        _ => throw new NotSupportedException()
                    };

                    track.Add(new TextEvent(text, MetaEventType.TextEvent, tickPos));
                }

                track.Add(new MetaEvent(MetaEventType.EndTrack, 0, track.Max(x => x.AbsoluteTime)));
                mid.AddTrack(track);
            }

            MidiFile.Export(exportMidPath, mid);
        }

        protected long FramePosToTicks(float framePos)
        {
            // Some position are negative
            if (framePos < 0.0f)
                framePos = 0.0f;

            // TODO: Read from base midi
            var ticksPerQuarter = 480;
            var mpq = 60000000 / 120;
            var fps = 30.0f;

            var seconds = framePos / fps;

            long absoluteTicks = (1000L * (long)(seconds * 1000) * ticksPerQuarter) / mpq;

            return absoluteTicks;
        }
    }
}
