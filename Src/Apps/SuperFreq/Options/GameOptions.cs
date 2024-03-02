using CommandLine;
using Mackiloha.IO;
using SuperFreq.Models;

namespace SuperFreq.Options;

public class GameOptions : BaseOptions
{
    protected readonly Platform[] SupportedPlatforms = [Platform.PS2, Platform.X360];

    [Option('m', "miloVersion", Default = 24, HelpText = "Milo archive version (10, 24, 25)")]
    public int MiloVersion { get; set; }

    [Option('b', "bigEndian", Default = false, HelpText = "Use big endian serialization")]
    public bool BigEndian { get; set; }

    [Option('p', "platform", Default = "ps2", HelpText = "Platform (ps2, ps3, x360, wii)")]
    public string PlatformString { get; set; }
    public Platform Platform { get; set; } = Platform.PS2;

    [Option('r', "preset", HelpText = "Game preset (gh1, gh2, gh80s, gh2_x360)")]
    public string Preset { get; set; }

    private Platform ParsePlatform(string value)
    {
        value = value?.Trim()?.ToLower();

        if (value is null)
            return Platform.PS2;

        Platform? platformInput = Enum.GetNames(typeof(Platform))
            .Where(x => x.ToLower() == value)
            .Select(y => (Platform?)Enum.Parse(typeof(Platform), y))
            .FirstOrDefault();

        if (platformInput.HasValue)
            return platformInput.Value;

        throw new Exception($"Preset of \"{value}\" not recognized");
    }

    public void UpdateOptions()
    {
        Platform = ParsePlatform(PlatformString);

        // Parse preset value
        var presetValue = Preset?.Trim().ToLower();
        if (presetValue is null)
        {
            // Do nothing
            return;
        }

        // Get preset
        var config = MiloConfig.Presets.FirstOrDefault(x => x.Games.Any(y => y == presetValue));
        if (config is null)
        {
            // Preset no found
            Log.Warning("Can't find preset for \"{preset}\"", presetValue);
            return;
        }

        Log.Information("Using preset for {presetName}", config.Name);

        // Updates options
        MiloVersion = config.MiloVersion;
        BigEndian = config.BigEndian;
        Platform = config.Platform;
    }

    public void VerifySupportedOptions()
    {
        if (MiloVersion > 25
            || BigEndian
            || !SupportedPlatforms.Contains(Platform))
        {
            Log.Warning("Selected options is an unsupported configuration. Compatibility is unverified.");
        }
    }

    public SystemInfo GetSystemInfo() => new SystemInfo()
    {
        Version = MiloVersion,
        BigEndian = BigEndian,
        Platform = Platform
    };
}
