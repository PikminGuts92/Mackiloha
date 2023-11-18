using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using SuperFreq.Apps;
using SuperFreq.Options;
using System.Diagnostics.CodeAnalysis;

namespace SuperFreq;

class Program
{
    // Fixes AOT for CommandLine
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Dir2MiloOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Milo2DirOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Png2TextureOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Texture2PngOptions))]
    static void Main(string[] args)
    {
        using var serviceProvider = CreateProvider();

        // TODO: Make pretty
        Parser.Default.ParseArguments<
            Dir2MiloOptions,
            Milo2DirOptions,
            Png2TextureOptions,
            Texture2PngOptions>(args)
            .WithParsed<Dir2MiloOptions>(serviceProvider.GetService<Dir2MiloApp>().Parse)
            .WithParsed<Milo2DirOptions>(serviceProvider.GetService<Milo2DirApp>().Parse)
            .WithParsed<Png2TextureOptions>(serviceProvider.GetService<Png2TextureApp>().Parse)
            .WithParsed<Texture2PngOptions>(serviceProvider.GetService<Texture2PngApp>().Parse)
            .WithNotParsed(errors => { });
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        Startup.ConfigureServices(services);

        return services.BuildServiceProvider();
    }
}
