using ArkHelper.Apps;
using ArkHelper.Options;
using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ArkHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            using var serviceProvider = CreateProvider();

            Parser.Default.ParseArguments<
                Ark2DirOptions,
                ArkCompareOptions,
                Dir2ArkOptions,
                FixHdrOptions,
                HashFinderOptions,
                PatchCreatorOptions>(args)
                .WithParsed<Ark2DirOptions>(serviceProvider.GetService<Ark2DirApp>().Parse)
                .WithParsed<ArkCompareOptions>(serviceProvider.GetService<ArkCompareApp>().Parse)
                .WithParsed<Dir2ArkOptions>(serviceProvider.GetService<Dir2ArkApp>().Parse)
                .WithParsed<FixHdrOptions>(serviceProvider.GetService<FixHdrApp>().Parse)
                .WithParsed<HashFinderOptions>(serviceProvider.GetService<HashFinderApp>().Parse)
                .WithParsed<PatchCreatorOptions>(serviceProvider.GetService<PatchCreatorApp>().Parse)
                .WithNotParsed(errors => { });
        }

        private static ServiceProvider CreateProvider()
        {
            var services = new ServiceCollection();
            Startup.ConfigureServices(services);

            return services.BuildServiceProvider();
        }
    }
}
