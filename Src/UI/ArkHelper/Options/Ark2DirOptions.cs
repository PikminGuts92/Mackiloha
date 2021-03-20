using CommandLine;

namespace ArkHelper.Options
{
    [Verb("ark2dir", HelpText = "Extracts content of milo ark to directory")]
    public class Ark2DirOptions
    {
        [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "dirPath", HelpText = "Path to output directory")]
        public string OutputPath { get; set; }

        [Option('s', "convertScripts", HelpText = "Convert dtb scripts to dta")]
        public bool ConvertScripts { get; set; }
        [Option('i', "indentSize", HelpText = "Tab indention size when converting scripts", Default = 3)]
        public int IndentSize { get; set; }

        [Option('t', "convertTextures", HelpText = "Convert textures to .png", Hidden = true)]
        public bool ConvertTextures { get; set; }

        [Option('m', "inflateMilos", HelpText = "Inflate milo archives (decompress)")]
        public bool InflateMilos { get; set; }

        [Option('x', "extractMilos", HelpText = "Extract milo archives (can't be used with decompress)", Hidden = true)]
        public bool ExtractMilos { get; set; }

        [Option('a', "extractAll", HelpText = "Extract everything")]
        public bool ExtractAll { get; set; }
    }
}
