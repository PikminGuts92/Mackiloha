using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace ArkHelper.Options
{
    [Verb("compare", HelpText = "Compares differences between two arks", Hidden = true)]
    public class ArkCompareOptions
    {
        [Value(0, Required = true, MetaName = "arkPath1", HelpText = "Path to ark 1 (hdr file)")]
        public string ArkPath1 { get; set; }

        [Value(1, Required = true, MetaName = "arkPath2", HelpText = "Path to ark 2 (hdr file)")]
        public string ArkPath2 { get; set; }
    }
}
