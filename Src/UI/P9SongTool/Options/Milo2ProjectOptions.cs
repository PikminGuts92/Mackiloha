using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace P9SongTool.Options
{
    [Verb("milo2proj", HelpText = "Create song project from input song milo archive")]
    public class Milo2ProjectOptions
    {
        [Value(0, Required = true, MetaName = "miloPath", HelpText = "Path to input milo archive")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "dirPath", HelpText = "Path to output directory")]
        public string OutputPath { get; set; }

        [Option('m', "midi", HelpText = "Base MIDI path to import tempo map (otherwise constant 120bpm will be used)")]
        public string BaseMidiPath { get; set; }

        [Usage(ApplicationAlias = "p9songtool.exe")]
        public static IEnumerable<Example> Examples
            => new[]
            {
                new Example("Convert milo archive to song project", new Milo2ProjectOptions
                {
                    InputPath = "temporarysec.milo",
                    OutputPath = "project_temporarysec",
                    BaseMidiPath = null
                }),
                new Example("Convert milo archive to song project w/ base .mid", new Milo2ProjectOptions
                {
                    InputPath = "temporarysec.milo",
                    OutputPath = "project_temporarysec",
                    BaseMidiPath = "temporarysec.mid"
                })
            };
    }
}
