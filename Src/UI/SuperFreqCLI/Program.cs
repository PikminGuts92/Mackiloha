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
            // TODO: Make pretty
            Parser.Default.ParseArguments<
                CryptOptions,
                Dir2MiloOptions,
                Milo2DirOptions,
                Milo2GLTFOptions,
                PngToTextureOptions,
                TextureToPngOptions>(args)
                .WithParsed<CryptOptions>(CryptOptions.Parse)
                .WithParsed<Dir2MiloOptions>(Dir2MiloOptions.Parse)
                .WithParsed<Milo2DirOptions>(Milo2DirOptions.Parse)
                .WithParsed<Milo2GLTFOptions>(Milo2GLTFOptions.Parse)
                .WithParsed<PngToTextureOptions>(PngToTextureOptions.Parse)
                .WithParsed<TextureToPngOptions>(TextureToPngOptions.Parse)
                .WithNotParsed(errors => { });
        }
    }
}
