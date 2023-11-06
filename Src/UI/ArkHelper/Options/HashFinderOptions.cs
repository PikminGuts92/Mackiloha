using CommandLine;

namespace ArkHelper.Options
{
    [Verb("hashfinder", HelpText = "Compute hashes for ark entries and find offsets in decrypted executable")]
    public class HashFinderOptions
    {
        [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)" )]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "exePath", HelpText = "Path to decrypted executable")]
        public string ExePath { get; set; }

        [Option('h', "hashPath", HelpText = "Path to file containing hash offsets", Required = true)]
        public string HashesPath { get; set; }
    }
}
