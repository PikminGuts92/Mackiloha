using CommandLine;

namespace ArkHelper.Options
{
    [Verb("fixhdr", HelpText = "Re-writes HDR (ark header) to hopefully fix sorting of entries", Hidden = true)]
    public class FixHdrOptions
    {
        [Value(0, Required = true, MetaName = "hdrPath", HelpText = "Path to hdr file")]
        public string InputPath { get; set; }

        [Value(1, Required = false, MetaName = "outputHdrPath", HelpText = "Optional path to output hdr file (otherwise modify in-place)")]
        public string OutputPath { get; set; }

        [Option('e', "forceEncrypt", HelpText = "Force encryption of HDR file")]
        public bool ForceEncryption { get; set; }
    }
}
