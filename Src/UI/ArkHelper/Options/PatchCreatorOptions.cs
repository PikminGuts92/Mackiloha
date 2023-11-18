using CommandLine;

namespace ArkHelper.Options;

[Verb("patchcreator", HelpText = "Creates a patch for arks")]
public class PatchCreatorOptions : BaseOptions
{
    [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)")]
    public string InputPath { get; set; }

    [Value(1, Required = false, MetaName = "exePath", HelpText = "Path to decrypted executable")]
    public string ExePath { get; set; }

    [Option('h', "hashPath", HelpText = "Path to file containing hash offsets", Required = false)]
    public string HashesPath { get; set; }

    [Option('a', "arkFilesPath", HelpText = "Path to directory for ark files", Required = true)]
    public string ArkFilesPath { get; set; }

    [Option('o', "outputPath", HelpText = "Path to directory for patch", Required = false)]
    public string OutputPath { get; set; }
}
