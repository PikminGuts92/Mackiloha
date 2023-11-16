using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;

namespace SuperFreq.Options;

[Verb("milo2dir", HelpText = "Extracts content of milo archive to directory")]
internal class Milo2DirOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "miloPath", HelpText = "Path to input milo archive")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "dirPath", HelpText = "Path to output directory")]
    public string OutputPath { get; set; }

    [Option("convertTextures", HelpText = "Automatically convert textures to PNG")]
    public bool ConvertTextures { get; set; }

    public static void Parse(Milo2DirOptions op)
    {
        op.UpdateOptions();
        op.VerifySupportedOptions();

        var appState = AppState.FromFile(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());
        appState.ExtractMiloContents(op.InputPath, op.OutputPath, op.ConvertTextures);

        Log.Information("Extracted milo contents to \"{outputPath}\"", op.OutputPath);
    }
}
