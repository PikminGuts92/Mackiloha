using Microsoft.Extensions.DependencyInjection;
using P9SongTool.Apps;

namespace P9SongTool
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Apps
            services.AddSingleton<Milo2ProjectApp>();
            services.AddSingleton<NewProjectApp>();
            services.AddSingleton<Project2MiloApp>();
        }
    }
}
