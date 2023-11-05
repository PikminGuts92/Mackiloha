using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Microsoft.Extensions.DependencyInjection;
using P9SongTool.Apps;
using P9SongTool.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace P9SongTool
{
    class Program
    {
        // Fixes AOT for CommandLine
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Milo2ProjectOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(NewProjectOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Project2MiloOptions))]
        static void Main(string[] args)
        {
            using var serviceProvider = CreateProvider();

            Parser.Default.ParseArguments<
                Milo2ProjectOptions,
                NewProjectOptions,
                Project2MiloOptions>(args)
                .WithParsed<Milo2ProjectOptions>(serviceProvider.GetService<Milo2ProjectApp>().Parse)
                .WithParsed<NewProjectOptions>(serviceProvider.GetService<NewProjectApp>().Parse)
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
