using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Microsoft.Extensions.DependencyInjection;
using P9SongTool.Apps;
using P9SongTool.Options;
using System;

namespace P9SongTool
{
    class Program
    {
        static void Main(string[] args)
        {
            using var serviceProvider = CreateProvider();

            Parser.Default.ParseArguments<
                Milo2ProjectOptions,
                Project2MiloOptions>(args)
                .WithParsed<Milo2ProjectOptions>(serviceProvider.GetService<Milo2ProjectApp>().Parse)
                .WithParsed<Project2MiloOptions>(serviceProvider.GetService<Project2MiloApp>().Parse)
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
