using CommandLine;

namespace ArkHelper.Options
{
    [Verb("dir2ark", HelpText = "Creates ark archive from input directory")]
    public class Dir2ArkOptions
    {
        [Value(0, Required = true, MetaName = "dirPath", HelpText = "Path to input directory")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "outputDir", HelpText = "Path to output directory")]
        public string OutputPath { get; set; }

        [Option('n', "name", HelpText = "Hdr/ark part(s) name", Default = "main")]
        public string ArkName { get; set; }

        [Option('v', "version", HelpText = "Ark version (supported: 3, 4, 5, 6)", Default = 3)]
        public int ArkVersion { get; set; }

        [Option('s', "sizeLimit", HelpText = "Size limit for ark parts", Default = 0x3FFFFFFFu)]
        public uint PartSizeLimit { get; set; }

        [Option('e', "encrypt", HelpText = "Encrypt hdr")]
        public bool Encrypt { get; set; }

        [Option('k', "key", HelpText = "Encryption key (one will be generated if not set)")]
        public uint? EncryptKey { get; set; }
    }
}
