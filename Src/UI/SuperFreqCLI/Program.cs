using System.Diagnostics.CodeAnalysis;
using CommandLine;
using SuperFreqCLI.Options;

namespace SuperFreqCLI
{
    class Program
    {
        // Fixes AOT for CommandLine
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CryptOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Dir2MiloOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Milo2DirOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PngToTextureOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TextureToPngOptions))]
        static void Main(string[] args)
        {
            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            // TODO: Make pretty
            Parser.Default.ParseArguments<
                CryptOptions,
                Dir2MiloOptions,
                Milo2DirOptions,
                PngToTextureOptions,
                TextureToPngOptions>(args)
                .WithParsed<CryptOptions>(CryptOptions.Parse)
                .WithParsed<Dir2MiloOptions>(Dir2MiloOptions.Parse)
                .WithParsed<Milo2DirOptions>(Milo2DirOptions.Parse)
                .WithParsed<PngToTextureOptions>(PngToTextureOptions.Parse)
                .WithParsed<TextureToPngOptions>(TextureToPngOptions.Parse)
                .WithNotParsed(errors => { });
        }
    }
}
