using NAudio.Midi;

namespace P9SongTool.Helpers;

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
        if (midPath is null)
        {
            // No mid path given
            Log.Warning("No base .mid file path given");
            return null;
        }
        else if (!File.Exists(midPath))
        {
            // Mid not found
            Log.Warning("Could not find \"{MidPath}\" to use as base .mid file, proceeding anyways", midPath);
            return null;
        }

        Log.Information("Using \"{MidPath}\" as base .mid file", midPath);
        return new MidiFile(midPath, strictChecking: false);
    }

    protected virtual List<(long tickPos, decimal framePos, int mpq)> CreateTempoMap()
    {
        var calculatedTempos = new List<(long tickPos, decimal framePos, int mpq)>();

        var fps = Framerate;
        var ticksPerQuarter = GetTicksPerQuarter();

        var currentTickPos = 0L;
        var currentFramePos = 0.0M;
        var currentMpq = 60_000_000 / 120;

        if (BaseMidi is null)
        {
            // No mid found, return default
            calculatedTempos.Add((currentTickPos, currentFramePos, currentMpq));
            return calculatedTempos;
        }

        var tempoChanges = BaseMidi.Events
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

        var fps = Framerate;
        var ticksPerQuarter = GetTicksPerQuarter();
        var currentTempo = TempoChanges.First();

        foreach (var change in TempoChanges.Skip(1))
        {
            if (change.framePos > (decimal)framePos)
                break;

            currentTempo = change;
        }

        var mpq = currentTempo.mpq;
        var deltaPosF = framePos - currentTempo.framePos;
        var seconds = deltaPosF / fps;

        long deltaTicks = (1000L * 1000L * (long)(seconds) * ticksPerQuarter) / mpq;
        return currentTempo.tickPos + deltaTicks;
    }

    internal decimal TickPosToFrames(long tickPos)
    {
        // Save the trouble of calculation
        if (tickPos <= 0L)
            return 0.0M;

        var fps = Framerate;
        var ticksPerQuarter = GetTicksPerQuarter();
        var currentTempo = TempoChanges.First();

        foreach (var change in TempoChanges.Skip(1))
        {
            if (change.tickPos > (long)tickPos)
                break;

            currentTempo = change;
        }

        var mpq = currentTempo.mpq;
        var deltaPosT = tickPos - currentTempo.tickPos;

        decimal deltaSeconds = (decimal)(mpq * deltaPosT) / (1000L * 1000L * ticksPerQuarter);
        return currentTempo.framePos + (deltaSeconds * fps);
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

    internal Dictionary<string, List<MidiEvent>> CreateMidiTracksDictionaryFromBase()
        => CreateMidiTracksFromBase()
            .Skip(1) // Skip tempo map
            .ToDictionary(
                x => x
                    .Where(y => (y is TextEvent te)
                        && te.MetaEventType == MetaEventType.SequenceTrackName)
                    .Select(y => y as TextEvent)
                    .First()
                    .Text,
                y => y);

    internal int GetTicksPerQuarter()
        => !(BaseMidi is null)
            ? BaseMidi.DeltaTicksPerQuarterNote
            : 480;
}
