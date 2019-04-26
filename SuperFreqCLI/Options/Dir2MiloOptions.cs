using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperFreqCLI.Options
{
    [Verb("dir2milo", HelpText = "Creates milo archive from input directory")]
    internal class Dir2MiloOptions
    {
        [Value(0, Required = true, MetaName = "dirPath", HelpText = "Path to input directory")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "miloPath", HelpText = "Path to output milo archive")]
        public string OutputPath { get; set; }
    }
}
