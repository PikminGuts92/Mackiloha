using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperFreqCLI.Options
{
    [Verb("milo2dir", HelpText = "Extracts content of milo archive to directory")]
    internal class Milo2DirOptions
    {
        [Value(0, Required = true, MetaName = "Input Path", MetaValue = "STRING", HelpText = "Path to input milo archive")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "Output Path", MetaValue = "STRING", HelpText = "Path to output directory")]
        public string OutputPath { get; set; }
    }
}
