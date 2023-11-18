using CommandLine;

namespace SuperFreq.Options;

[Verb("milo2dir", HelpText = "Extracts content of milo archive to directory")]
public class Milo2DirOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "miloPath", HelpText = "Path to input milo archive")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "dirPath", HelpText = "Path to output directory")]
    public string OutputPath { get; set; }

    [Option('t', "convertTextures", HelpText = "Automatically convert textures to PNG")]
    public bool ConvertTextures { get; set; }
}
