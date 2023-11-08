using CommandLine;
using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;

namespace SuperFreqCLI.Options;

[Verb("tex2png", HelpText = "Converts HMX texture to png (beta feature)")]
internal class TextureToPngOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "texPath", HelpText = "Path to input texture")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "pngPath", HelpText = "Path to output png")]
    public string OutputPath { get; set; }

    public static void Parse(TextureToPngOptions op)
    {
        op.UpdateOptions();

        var appState = AppState.FromFile(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());

        var serializer = appState.GetSerializer();
        var bitmap = serializer.ReadFromFile<HMXBitmap>(op.InputPath);
        bitmap.SaveAs(appState.SystemInfo, op.OutputPath);
    }
}
