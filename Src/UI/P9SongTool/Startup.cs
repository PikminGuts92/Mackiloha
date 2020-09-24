using Microsoft.Extensions.DependencyInjection;
using P9SongTool.Apps;
using System;
using System.Collections.Generic;
using System.Text;

namespace P9SongTool
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Apps
            services.AddSingleton<Milo2ProjectApp>();
            services.AddSingleton<Project2MiloApp>();
        }
    }
}
