using System;
using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using ArkHelper.Options;

namespace ArkHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                ArkCompareOptions,
                ArkExtractOptions,
                FixHdrOptions,
                HashFinderOptions,
                PatchCreatorOptions>(args)
                .WithParsed<ArkCompareOptions>(ArkCompareOptions.Parse)
                .WithParsed<ArkExtractOptions>(ArkExtractOptions.Parse)
                .WithParsed<FixHdrOptions>(FixHdrOptions.Parse)
                .WithParsed<HashFinderOptions>(HashFinderOptions.Parse)
                .WithParsed<PatchCreatorOptions>(PatchCreatorOptions.Parse)
                .WithNotParsed(errors => { });
        }
    }
}
