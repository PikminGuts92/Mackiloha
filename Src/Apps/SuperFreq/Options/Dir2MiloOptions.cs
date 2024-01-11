using CommandLine;

namespace SuperFreq.Options;

[Verb("dir2milo", HelpText = "Creates milo archive from input directory")]
public class Dir2MiloOptions : GameOptions
{
    [Value(0, Required = true, MetaName = "dirPath", HelpText = "Path to input directory")]
    public string InputPath { get; set; }

    [Value(1, Required = true, MetaName = "miloPath", HelpText = "Path to output milo archive")]
    public string OutputPath { get; set; }
}
