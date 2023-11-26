using CommandLine;

namespace SuperFreq.Options;

[Verb("png2tex", HelpText = "Converts HMX texture to png")]
public class Png2TextureOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "pngPath", HelpText = "Path to input png")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "texPath", HelpText = "Path to output texture")]
    public string OutputPath { get; set; }
}
