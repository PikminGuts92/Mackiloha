using CommandLine;

namespace P9SongTool.Options
{
    [Verb("proj2milo", HelpText = "Build song milo archive from input project")]
    public class Project2MiloOptions
    {
        [Value(0, Required = true, MetaName = "dirPath", HelpText = "Path to input project directory")]
        
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "miloPath", HelpText = "Path to output milo archive")]
        public string OutputPath { get; set; }

        [Option('u', "uncompressed", HelpText = "Enable to leave output milo archive uncompressed")]
        public bool UncompressedMilo { get; set; }
    }
}
