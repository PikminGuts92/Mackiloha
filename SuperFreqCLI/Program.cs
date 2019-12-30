using System;
using System.IO;
using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreqCLI.Options;

namespace SuperFreqCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Dir2MiloOptions, FixHdrOptions, HashFinderOptions, Milo2DirOptions, Milo2GLTFOptions, PatchCreatorOptions, PngToTextureOptions, TextureToPngOptions>(args)
                .WithParsed<Dir2MiloOptions>(Dir2MiloOptions.Parse)
                .WithParsed<FixHdrOptions>(FixHdrOptions.Parse)
                .WithParsed<HashFinderOptions>(HashFinderOptions.Parse)
                .WithParsed<Milo2DirOptions>(Milo2DirOptions.Parse)
                .WithParsed<Milo2GLTFOptions>(Milo2GLTFOptions.Parse)
                .WithParsed<PatchCreatorOptions>(PatchCreatorOptions.Parse)
                .WithParsed<PngToTextureOptions>(PngToTextureOptions.Parse)
                .WithParsed<TextureToPngOptions>(TextureToPngOptions.Parse)
                .WithNotParsed(errors => { });
        }
    }
}
