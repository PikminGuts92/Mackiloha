using ArkHelper.Apps;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHelper
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Apps
            services.AddSingleton<Ark2DirApp>();
            services.AddSingleton<ArkCompareApp>();
            services.AddSingleton<FixHdrApp>();
            services.AddSingleton<HashFinderApp>();
            services.AddSingleton<PatchCreatorApp>();
        }
    }
}
