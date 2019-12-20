using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using Mackiloha.Ark;
using Mackiloha.IO;

namespace SuperFreqCLI.Options
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

        public static void Parse(FixHdrOptions op)
        {
            if (op.OutputPath == null)
                op.OutputPath = op.InputPath;

            var ark = ArkFile.FromFile(op.InputPath);
            ark.Encrypted = ark.Encrypted || op.ForceEncryption; // Force encryption
            ark.WriteHeader(op.OutputPath);
        }
    }
}
