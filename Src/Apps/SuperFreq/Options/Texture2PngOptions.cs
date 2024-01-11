using CommandLine;

namespace SuperFreq.Options;

[Verb("tex2png", HelpText = "Converts HMX texture to png")]
public class Texture2PngOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "texPath", HelpText = "Path to input texture")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "pngPath", HelpText = "Path to output png")]
    public string OutputPath { get; set; }
}
