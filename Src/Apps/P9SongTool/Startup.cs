using Microsoft.Extensions.DependencyInjection;
using P9SongTool.Apps;
using P9SongTool.Helpers;

namespace P9SongTool;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Helpers
        services.AddSingleton<ILogManager, LogManager>();

        // Apps
        services.AddSingleton<Milo2ProjectApp>();
        services.AddSingleton<NewProjectApp>();
        services.AddSingleton<Project2MiloApp>();
    }
}
